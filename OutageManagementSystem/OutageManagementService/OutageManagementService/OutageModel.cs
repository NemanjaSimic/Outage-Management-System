using OMSCommon.Mappers;
using OMSCommon.OutageDatabaseModel;
using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.OutageService.Interface;
using Outage.Common.OutageService.Model;
using Outage.Common.PubSub.OutageDataContract;
using Outage.Common.ServiceContracts.CalculationEngine;
using Outage.Common.ServiceContracts.GDA;
using Outage.Common.ServiceContracts.PubSub;
using Outage.Common.ServiceContracts.SCADA;
using Outage.Common.ServiceProxies;
using Outage.Common.ServiceProxies.CalcualtionEngine;
using Outage.Common.ServiceProxies.PubSub;
using OutageDatabase;
using OutageDatabase.Repository;
using OutageManagementService.ScadaSubscriber;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using AutoResetEvent = System.Threading.AutoResetEvent;

namespace OutageManagementService
{
    public class CancelationObject 
    {
        public bool CancelationSignal { get; set; }
    }

    public sealed class OutageModel : IDisposable
    {
        private OutageTopologyModel topologyModel;

        public OutageTopologyModel TopologyModel
        {
            get
            {
                return topologyModel;
            }
            private set
            {
                topologyModel = value;
            }
        }

        private ILogger logger;
        private ProxyFactory proxyFactory;
        private OutageMessageMapper outageMessageMapper;
        private ModelResourcesDesc modelResourcesDesc;

        private UnitOfWork dbContext;
        private UnitOfWork transactionDbContext;
        
        public ConcurrentQueue<long> EmailMsg;
        public List<long> CalledOutages;
        private Dictionary<DeltaOpType, List<long>> modelChanges;

        private ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        public OutageModel()
        {
            proxyFactory = new ProxyFactory();
            outageMessageMapper = new OutageMessageMapper();
            modelResourcesDesc = new ModelResourcesDesc();

            dbContext = new UnitOfWork();

            CalledOutages = new List<long>();
            EmailMsg = new ConcurrentQueue<long>();

            ImportTopologyModel();
        }

        #region IDisposable
        public void Dispose()
        {
            dbContext.Dispose();
        }
        #endregion

        #region IModelUpdateNotificationContract
        public bool Notify(Dictionary<DeltaOpType, List<long>> modelChanges)
        {
            this.modelChanges = modelChanges;
            return true;
        }
        #endregion

        #region ITransactionActorContract

        public bool Prepare()
        {
            bool success = false;
            transactionDbContext = new UnitOfWork();
            Dictionary<long, ResourceDescription> resourceDescriptions = GetExtentValues(ModelCode.ENERGYCONSUMER, modelResourcesDesc.GetAllPropertyIds(ModelCode.ENERGYCONSUMER));

            List<Consumer> consumersInDb = transactionDbContext.ConsumerRepository.GetAll().ToList();

            foreach(Consumer consumer in consumersInDb)
            {
                if (modelChanges[DeltaOpType.Delete].Contains(consumer.ConsumerId))
                {
                    transactionDbContext.ConsumerRepository.Remove(consumer);
                }
                else if (modelChanges[DeltaOpType.Update].Contains(consumer.ConsumerId))
                {
                    consumer.ConsumerMRID = resourceDescriptions[consumer.ConsumerId].GetProperty(ModelCode.IDOBJ_MRID).AsString(); //TODO other prop, when added in model
                }
            }

            foreach(long gid in modelChanges[DeltaOpType.Insert])
            {
                ModelCode type = modelResourcesDesc.GetModelCodeFromId(gid);

                if (type == ModelCode.ENERGYCONSUMER)
                {
                    ResourceDescription resourceDescription = resourceDescriptions[gid];

                    if (resourceDescription != null)
                    {
                        Consumer consumer = new Consumer
                        {
                            ConsumerId = resourceDescription.Id,
                            ConsumerMRID = resourceDescription.GetProperty(ModelCode.IDOBJ_MRID).AsString(),
                            FirstName = "Added", //TODO: resourceDescription.GetProperty(ModelCode.ENERGYCONSUMER_FIRSTNAME).AsString()
                            LastName = "Consumer" //TODO: resourceDescription.GetProperty(ModelCode.ENERGYCONSUMER_LASTNAME).AsString() other prop, when added in model
                        };

                        transactionDbContext.ConsumerRepository.Add(consumer);
                    }
                    else
                    {
                        Logger.LogWarn($"Consumer with gid 0x{gid:X16} is not in network model");
                    }
                }
            }

            success = true;

            return success;
        }


        public void Commit()
        {
            try
            {
                transactionDbContext.Complete();
            }
            catch (Exception e)
            {
                string message = "OutageModel::Commit method => exception on Complete()";
                Logger.LogError(message, e);
                Console.WriteLine($"{message}, Message: {e.Message})");
            }
            finally
            {
                transactionDbContext.Dispose();
                transactionDbContext = null;
            }
        }

        public void Rollback()
        {
            transactionDbContext.Dispose();
            transactionDbContext = null;
            modelChanges = null;
        }
        #endregion

        #region IOutageLifecycleContract
        public bool ReportPotentialOutage(long gid)
        {
            bool success = false;

            List<long> affectedConsumersIds = new List<long>();

            //TODO: special case: potenitial outage is remote (and closed)...

            affectedConsumersIds = GetAffectedConsumers(gid);

            if (affectedConsumersIds.Count == 0)
            {
                Logger.LogInfo("There is no affected consumers => outage report is not valid.");
                return false;
            }

            ActiveOutage activeOutageDb = null;

            if (dbContext.ActiveOutageRepository.Find(o => o.OutageElementGid == gid).FirstOrDefault() != null)
            {
                Logger.LogWarn($"Reported malfunction on element with gid: 0x{gid:x16} has already been reported.");
                return false;
            }

            List<Consumer> consumersDb = GetAffectedConsumersFromDatabase(affectedConsumersIds);

            if (consumersDb.Count != affectedConsumersIds.Count)
            {
                Logger.LogWarn("Some of affected consumers are not present in database");
                return false;
            }

            ActiveOutage createdActiveOutage = new ActiveOutage
            {
                AffectedConsumers = consumersDb,
                OutageState = ActiveOutageState.CREATED,
                OutageElementGid = gid, //TODO: remove OutageElementGid from this initialization
                ReportTime = DateTime.UtcNow,
                //TODO: add DefaultIsolationPoints = new string from gid
            };

            activeOutageDb = dbContext.ActiveOutageRepository.Add(createdActiveOutage);

            try
            {
                dbContext.Complete();
                Logger.LogDebug($"Outage on element with gid: 0x{activeOutageDb.OutageElementGid:x16} is successfully stored in database.");
                success = true;
            }
            catch (Exception e)
            {
                string message = "OutageModel::ReportPotentialOutage method => exception on Complete()";
                Logger.LogError(message, e);
                Console.WriteLine($"{message}, Message: {e.Message})");

                //TODO: da li je dobar handle?
                dbContext.Dispose();
                dbContext = new UnitOfWork();
                success = false;
            }

            if (success && activeOutageDb != null)
            {
                try
                {
                    success = PublishActiveOutage(Topic.ACTIVE_OUTAGE, outageMessageMapper.MapActiveOutage(activeOutageDb));
                    
                    if(success)
                    {
                        Logger.LogInfo($"Outage on element with gid: 0x{activeOutageDb.OutageElementGid:x16} is successfully published");
                    }
                }
                catch (Exception e) //TODO: Exception over proxy or enum...
                {
                    Logger.LogError("OutageModel::ReportPotentialOutage => exception on PublishActiveOutage()", e);
                    success = false;
                }
            }

            return success;
        }

        public bool IsolateOutage(long outageId)
        {
            bool success = false;
            using (OutageContext db = new OutageContext())
            {
                ActiveOutage outageToIsolate = db.ActiveOutages.Find(outageId);
                if (outageToIsolate != null)
                {
                    if (outageToIsolate.OutageState == ActiveOutageState.CREATED)
                    {
                        try
                        {
                            success = StartIsolationAlgorthm(outageToIsolate);
                        }
                        catch (Exception e)
                        {
                            success = false;
                            Logger.LogError("Exception on StartIsolationAlgorthm() method.", e);
                        }

                        if (success)
                        {
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        Logger.LogWarn($"Outage with id 0x{outageId:X16} is in state {outageToIsolate.OutageState}, and thus cannot be isolated.");
                    }
                }
                else
                {
                    Logger.LogWarn($"Outage with id 0x{outageId:X16} is not found in database.");
                    success = false;
                }
            }

            return success;
        }

        public bool SendRepairCrew(long outageId)
        {
            bool success;

            ActiveOutage outageDB = null;

            try
            {
                outageDB = dbContext.ActiveOutageRepository.Get(outageId);
            }
            catch (Exception e)
            {
                string message = "OutageModel::SendRepairCrew => exception in UnitOfWork.ActiveOutageRepository.Get()";
                Logger.LogError(message, e);
                throw e;
            }
            
            if(outageDB == null)
            {
                Logger.LogError($"Outage with id 0x{outageId:X16} is not found in database.");
                return false;
            }

            if(outageDB.OutageState != ActiveOutageState.ISOLATED)
            {
                Logger.LogError($"Outage with id 0x{outageId:X16} is in state {outageDB.OutageState}, and thus repair crew can not be sent. (Expected state: {ActiveOutageState.CREATED})");
                return false;
            }

            success = false;
            return success;
        }

        public bool SendLocationIsolationCrew(long outageId)
        {
            throw new NotImplementedException();
        }

        public bool ValidateResolveConditions(long outageId)
        {
            throw new NotImplementedException();
        }

        public bool ResolveOutage(long outageId)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Private Methods
        private void ImportTopologyModel()
        {
            using (OMSTopologyServiceProxy omsTopologyProxy = proxyFactory.CreateProxy<OMSTopologyServiceProxy, ITopologyOMSService>(EndpointNames.TopologyOMSServiceEndpoint))
            {
                if (omsTopologyProxy == null)
                {
                    string message = "From method ImportTopologyModel(): TopologyServiceProxy is null.";
                    logger.LogError(message);
                    throw new NullReferenceException(message);
                }

                TopologyModel = (OutageTopologyModel)omsTopologyProxy.GetOMSModel();
            }
        }

        private List<Consumer> GetAffectedConsumersFromDatabase(List<long> affectedConsumersIds)
        {
            List<Consumer> affectedConsumers = new List<Consumer>();

            foreach (long affectedConsumerId in affectedConsumersIds)
            {
                Consumer affectedConsumer = dbContext.ConsumerRepository.Get(affectedConsumerId);

                if (affectedConsumer == null)
                {
                    break;
                }

                affectedConsumers.Add(affectedConsumer);
            }

            return affectedConsumers;
        }

        private bool PublishActiveOutage(Topic topic, OutageMessage outageMessage)
        {
            bool success;

            OutagePublication outagePublication = new OutagePublication(topic, outageMessage);

            using (PublisherProxy publisherProxy = proxyFactory.CreateProxy<PublisherProxy, IPublisher>(EndpointNames.PublisherEndpoint))
            {
                if (publisherProxy == null)
                {
                    string errMsg = "Publisher proxy is null";
                    Logger.LogWarn(errMsg);
                    throw new NullReferenceException(errMsg);
                }

                try
                {
                    publisherProxy.Publish(outagePublication, "OUTAGE_PUBLISHER");
                    Logger.LogInfo($"Outage service published data from topic: {outagePublication.Topic}");
                    success = true;
                }
                catch (Exception e)
                {
                    string message = $"OutageModel::PublishActiveOutage => exception on PublisherProxy.Publish()";
                    Logger.LogError(message, e);
                    success = false;
                }
            }

            return success;
        }

        private bool StartIsolationAlgorthm(ActiveOutage outageToIsolate)
        {
            bool isIsolated = false;


            List<long> defaultIsolationPoints = GetElementIdsFromString(outageToIsolate.DefaultIsolationPoints);

            if (defaultIsolationPoints.Count > 0 && defaultIsolationPoints.Count < 3)
            {
                long headBreaker = -1;
                long recloser = -1;
                try
                {
                    bool isFirstBreakerRecloser = CheckIfBreakerIsRecloser(defaultIsolationPoints[0]);
                    //TODO is second recloser (future)
                    if (defaultIsolationPoints.Count == 2)
                    {
                        if (isFirstBreakerRecloser)
                        {
                            headBreaker = defaultIsolationPoints[1];
                            recloser = defaultIsolationPoints[0];
                        }
                        else
                        {
                            headBreaker = defaultIsolationPoints[0];
                            recloser = defaultIsolationPoints[1];
                        }
                    }
                    else
                    {
                        if (!isFirstBreakerRecloser)
                        {
                            headBreaker = defaultIsolationPoints[0];
                        }
                        else
                        {
                            Logger.LogWarn($"Invalid state: breaker with id 0x{defaultIsolationPoints[0]:X16} is the only default isolation element, but it is also a recloser.");
                            isIsolated = false;
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.LogWarn("Exception on method CheckIfBreakerIsRecloser()", e);

                }


                if (headBreaker != -1)
                {
                    ModelCode mc = modelResourcesDesc.GetModelCodeFromId(headBreaker);
                    if (mc == ModelCode.BREAKER)
                    {
                        Logger.LogInfo($"Head breaker id: 0x{headBreaker:X16}, recloser id: 0x{recloser:X16} (-1 if no recloser).");

                        //ALGORITHM
                        AutoResetEvent autoResetEvent = new AutoResetEvent(false);
                        CancelationObject cancelationObject = new CancelationObject() { CancelationSignal = false };
                        Timer timer = InitalizeAlgorthmTimer(cancelationObject, autoResetEvent);
                        ScadaNotification scadaNotification = new ScadaNotification("OutageModel_SCADA_Subscriber", new OutageIsolationAlgorithm.OutageIsolationAlgorithmParameters(headBreaker, recloser, autoResetEvent));
                        SubscriberProxy subscriberProxy = proxyFactory.CreateProxy<SubscriberProxy, ISubscriber>(scadaNotification, EndpointNames.SubscriberEndpoint);
                        subscriberProxy.Subscribe(Topic.SWITCH_STATUS);

                        long currentBreakerId = headBreaker;

                        while (!cancelationObject.CancelationSignal)
                        {
                            if (TopologyModel.OutageTopology.ContainsKey(currentBreakerId))
                            {
                                currentBreakerId = TopologyModel.OutageTopology[currentBreakerId].SecondEnd.Where(element => modelResourcesDesc.GetModelCodeFromId(element) == ModelCode.BREAKER).FirstOrDefault();
                                if (currentBreakerId == 0 || currentBreakerId == recloser)
                                {
                                    //TODO: planned outage
                                    string message = "End of the feeder, no outage detected.";
                                    Logger.LogWarn(message);
                                    isIsolated = false;
                                    subscriberProxy.Close();
                                    throw new Exception(message);
                                }
                                //TODO: SCADACommand
                                SendSCADACommand(currentBreakerId, DiscreteCommandingType.OPEN);
                                timer.Start();
                                autoResetEvent.WaitOne();
                                if (timer.Enabled)
                                {
                                    timer.Stop();
                                    SendSCADACommand(currentBreakerId, DiscreteCommandingType.CLOSE);
                                }

                            }
                        }

                        long nextBreakerId = TopologyModel.OutageTopology[currentBreakerId].SecondEnd.Where(element => modelResourcesDesc.GetModelCodeFromId(element) == ModelCode.BREAKER).FirstOrDefault();
                        if (currentBreakerId != 0 && currentBreakerId != recloser)
                        {
                            outageToIsolate.OptimumIsolationPoints = $"{currentBreakerId}|{nextBreakerId}";
                            //TODO: SCADA Command
                            SendSCADACommand(currentBreakerId, DiscreteCommandingType.OPEN);
                            outageToIsolate.IsolatedTime = DateTime.UtcNow;
                            Logger.LogInfo($"Isolation of outage with id {outageToIsolate.OutageId}. Optimum isolation points: {currentBreakerId} and {nextBreakerId}");
                            isIsolated = true;
                        }
                        else
                        {
                            string message = "End of the feeder, no outage detected.";
                            Logger.LogWarn(message);
                            isIsolated = false;
                            subscriberProxy.Close();
                            throw new Exception(message);
                        }
                        subscriberProxy.Close();

                    }
                    else
                    {
                        Logger.LogWarn($"Head breaker type is {mc}, not a BREAKER.");
                        isIsolated = false;
                    }

                }
                else
                {
                    Logger.LogWarn("Head breaker not found.");
                    isIsolated = false;
                }
            }
            else
            {
                Logger.LogWarn($"Number of defaultIsolationPoints ({defaultIsolationPoints.Count}) is out of range [1, 2].");
                isIsolated = false;
            }



            return isIsolated;
        }

        private void SendSCADACommand(long currentBreakerId, DiscreteCommandingType discreteCommandingType)
        {
            long measrement = -1;
            using (MeasurementMapProxy measurementMapProxy = proxyFactory.CreateProxy<MeasurementMapProxy, IMeasurementMapContract>(EndpointNames.MeasurementMapEndpoint))
            {
                List<long> measuremnts = new List<long>();
                try
                {
                    measuremnts = measurementMapProxy.GetMeasurementsOfElement(currentBreakerId);

                }
                catch (Exception e)
                {
                    //Logger.LogError("Error on GetMeasurementsForElement() method", e);
                    throw e;
                }

                if (measuremnts.Count > 0)
                {
                    measrement = measuremnts[0];
                }

            }

            if (measrement != -1)
            {
                using (SCADACommandProxy scadaCommandProxy = proxyFactory.CreateProxy<SCADACommandProxy, ISCADACommand>(EndpointNames.SCADACommandService))
                {
                    try
                    {
                        bool success = scadaCommandProxy.SendDiscreteCommand(measrement, (ushort)discreteCommandingType);
                        if (success)
                        {
                            if (discreteCommandingType == DiscreteCommandingType.OPEN)
                            {
                                //TODO: add at list
                            }
                            else if (discreteCommandingType == DiscreteCommandingType.CLOSE)
                            {
                                //TODO: remove from list
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
            }
        }

        private Timer InitalizeAlgorthmTimer(CancelationObject cancelationSignal, AutoResetEvent autoResetEvent)
        {
            Timer timer = new Timer();
            timer.Elapsed += (sender, e) => AlgorthmTimerElapsedCallback(sender, e, cancelationSignal, autoResetEvent);
            timer.Interval = 10000; //TODO: Config
            timer.AutoReset = false;

            return timer;
        }

        private void AlgorthmTimerElapsedCallback(object sender, ElapsedEventArgs e, CancelationObject cancelationSignal, AutoResetEvent autoResetEvent)
        {
            cancelationSignal.CancelationSignal = true;
            autoResetEvent.Set();
        }

        private List<long> GetElementIdsFromString(string elementIdsString)
        {
            List<long> elementIds = new List<long>();

            string[] separatedElementIdsString = elementIdsString.Split('|');

            foreach (string elementIdString in separatedElementIdsString)
            {
                if (long.TryParse(elementIdString, out long elementId))
                {
                    elementIds.Add(elementId);
                }
                else
                {
                    Logger.LogWarn($"Error while parsing elementIdString: 0x{elementIdString:X16}");
                    elementIds.Clear();
                    break;
                }
            }


            return elementIds;
        }

        private bool CheckIfBreakerIsRecloser(long elementId)
        {
            bool isRecloser = false;

            try
            {
                using (NetworkModelGDAProxy gda = proxyFactory.CreateProxy<NetworkModelGDAProxy, INetworkModelGDAContract>(EndpointNames.NetworkModelGDAEndpoint))
                {
                    ResourceDescription resourceDescription = gda.GetValues(elementId, new List<ModelCode>() { ModelCode.BREAKER_NORECLOSING });

                    Property property = resourceDescription.GetProperty(ModelCode.BREAKER_NORECLOSING);

                    if (property != null)
                    {
                        isRecloser = !property.AsBool();
                    }
                    else
                    {
                        throw new Exception($"Element with id 0x{elementId:X16} is not a breaker.");
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return isRecloser;
        }

        private List<long> GetAffectedConsumers(long potentialOutageGid)
        {
            List<long> affectedConsumers = new List<long>();
            Stack<long> nodesToBeVisited = new Stack<long>();
            nodesToBeVisited.Push(potentialOutageGid);
            HashSet<long> visited = new HashSet<long>();


            while (nodesToBeVisited.Count > 0)
            {
                long currentNode = nodesToBeVisited.Pop();

                if (!visited.Contains(currentNode))
                {
                    visited.Add(currentNode);

                    if (topologyModel.OutageTopology.ContainsKey(currentNode))
                    {
                        IOutageTopologyElement topologyElement = topologyModel.OutageTopology[currentNode];

                        if (topologyElement.SecondEnd.Count == 0 && topologyElement.DmsType == "ENERGYCONSUMER")
                        {
                            affectedConsumers.Add(currentNode);
                        }

                        foreach (long adjNode in topologyElement.SecondEnd)
                        {
                            nodesToBeVisited.Push(adjNode);
                        }
                    }
                    else
                    {
                        //TOOD
                        string message = $"GID: 0x{currentNode:X16} not found in topologyModel.OutageTopology dictionary....";
                        Logger.LogError(message);
                        Console.WriteLine(message);
                    }
                }
            }

            return affectedConsumers;
        }
        #endregion

        #region GDAHelper
        private Dictionary<long, ResourceDescription> GetExtentValues(ModelCode entityType, List<ModelCode> propIds)
        {
            int iteratorId;

            using (NetworkModelGDAProxy gdaQueryProxy = proxyFactory.CreateProxy<NetworkModelGDAProxy, INetworkModelGDAContract>(EndpointNames.NetworkModelGDAEndpoint))
            {
                if (gdaQueryProxy == null)
                {
                    string message = "GetExtentValues() => NetworkModelGDAProxy is null.";
                    Logger.LogError(message);
                    throw new NullReferenceException(message);
                }

                try
                {
                    iteratorId = gdaQueryProxy.GetExtentValues(entityType, propIds);
                }
                catch (Exception e)
                {
                    string message = $"Failed to get extent values for dms type {entityType}.";
                    Logger.LogError(message, e);
                    throw e;
                }
            }

            return ProcessIterator(iteratorId);
        }

        private Dictionary<long, ResourceDescription> ProcessIterator(int iteratorId)
        {
            //TODO: mozda vec ovde napakovati dictionary<long, rd> ?
            int resourcesLeft;
            int numberOfResources = 10000;
            Dictionary<long, ResourceDescription> resourceDescriptions;

            using (NetworkModelGDAProxy gdaQueryProxy = proxyFactory.CreateProxy<NetworkModelGDAProxy, INetworkModelGDAContract>(EndpointNames.NetworkModelGDAEndpoint))
            {
                if (gdaQueryProxy == null)
                {
                    string message = "ProcessIterator() => NetworkModelGDAProxy is null.";
                    Logger.LogError(message);
                    throw new NullReferenceException(message);
                }

                try
                {
                    resourcesLeft = gdaQueryProxy.IteratorResourcesTotal(iteratorId);
                    resourceDescriptions = new Dictionary<long, ResourceDescription>(resourcesLeft);

                    while (resourcesLeft > 0)
                    {
                        List<ResourceDescription> resources = gdaQueryProxy.IteratorNext(numberOfResources, iteratorId);
                        
                        foreach (ResourceDescription resource in resources)
                        {
                            resourceDescriptions.Add(resource.Id, resource);
                        }

                        resourcesLeft = gdaQueryProxy.IteratorResourcesLeft(iteratorId);
                    }

                    gdaQueryProxy.IteratorClose(iteratorId);
                }
                catch (Exception e)
                {
                    string message = $"Failed to retrieve all Resourse descriptions with iterator {iteratorId}.";
                    Logger.LogError(message, e);
                    throw e;
                }
            }

            return resourceDescriptions;
        }

        #endregion
    }
}

using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.OutageService.Interface;
using Outage.Common.OutageService.Model;
using Outage.Common.PubSub.OutageDataContract;
using Outage.Common.ServiceContracts.CalculationEngine;
using Outage.Common.ServiceContracts.GDA;
using Outage.Common.ServiceContracts.OMS;
using Outage.Common.ServiceContracts.PubSub;
using Outage.Common.ServiceContracts.SCADA;
using Outage.Common.ServiceProxies;
using Outage.Common.ServiceProxies.CalcualtionEngine;
using Outage.Common.ServiceProxies.PubSub;
using OutageDatabase;
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

    public class OutageModel
    {
        private OutageTopologyModel topologyModel;

        public OutageTopologyModel TopologyModel
        {
            get
            {
                return topologyModel;
            }
            protected set
            {
                topologyModel = value;
            }
        }

        private ILogger logger;
        private ProxyFactory proxyFactory;

        public ConcurrentQueue<long> EmailMsg;
        public List<long> CalledOutages;
        private Dictionary<DeltaOpType, List<long>> modelChanges;
        private ModelResourcesDesc modelResourcesDesc;
        private OutageContext transactionOutageContext;

        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        public OutageModel()
        {
            EmailMsg = new ConcurrentQueue<long>();
            CalledOutages = new List<long>();
            modelResourcesDesc = new ModelResourcesDesc();
            proxyFactory = new ProxyFactory();

            ImportTopologyModel();
        }

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
            transactionOutageContext = new OutageContext();
            Dictionary<long, ResourceDescription> resourceDescriptions = GetExtentValues(ModelCode.ENERGYCONSUMER, modelResourcesDesc.GetAllPropertyIds(ModelCode.ENERGYCONSUMER));

            List<Consumer> consumersInDb = transactionOutageContext.Consumers.ToList();

            foreach(Consumer consumer in consumersInDb)
            {
                if (modelChanges[DeltaOpType.Delete].Contains(consumer.ConsumerId))
                {
                    transactionOutageContext.Consumers.Remove(consumer);
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
                        Consumer consumer = new Consumer();
                        consumer.ConsumerId = resourceDescription.Id;
                        consumer.ConsumerMRID = resourceDescription.GetProperty(ModelCode.IDOBJ_MRID).AsString();
                        consumer.FirstName = "Added";
                        consumer.LastName = "Consumer"; //TODO other prop, when added in model

                        transactionOutageContext.Consumers.Add(consumer);
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
            transactionOutageContext.SaveChanges();
            transactionOutageContext.Dispose();
            transactionOutageContext = null;
        }

        public void Rollback()
        {
            transactionOutageContext.Dispose();
            transactionOutageContext = null;
            modelChanges = null;
        }
        #endregion


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

        #region IOutageLifecycleContract
        public bool ReportPotentialOutage(long gid)
        {
            bool success = false;

            List<long> affectedConsumersIds = new List<long>();

            //TODO: special case: potenitial outage is remote (and closed)...

            affectedConsumersIds = GetAffectedConsumers(gid);

            if (affectedConsumersIds.Count > 0)
            {
                ActiveOutage activeOutage = null;
                using (OutageContext db = new OutageContext())
                {
                    try
                    {
                        if (db.GetActiveOutage(gid) == null)
                        {
                            List<Consumer> consumers = GetAffectedConsumersFromDatabase(affectedConsumersIds, db);
                            if (consumers.Count == affectedConsumersIds.Count)
                            {
                                activeOutage = db.ActiveOutages.Add(new ActiveOutage { AffectedConsumers = consumers, OutageState = OutageState.CREATED, OutageElementGid = gid, ReportTime = DateTime.UtcNow });
                                db.SaveChanges();
                            }
                            else
                            {
                                Logger.LogWarn("Some of affected consumers are not present in database");
                            }
                            Logger.LogDebug($"Outage on element with gid: 0x{activeOutage.OutageElementGid:x16} is successfully stored in database.");
                        }
                        else
                        {
                            Logger.LogWarn($"Reported element with gid: 0x{activeOutage.OutageElementGid:x16} has already been reported.");
                        }

                    }
                    catch (Exception e)
                    {
                        activeOutage = null;
                        Logger.LogError("Error while adding reported outage into database.", e);
                    }
                }

                if (activeOutage != null)
                {
                    try
                    {
                        PublishActiveOutage(Topic.ACTIVE_OUTAGE, activeOutage);
                        Logger.LogInfo($"Outage on element with gid: 0x{activeOutage.OutageElementGid:x16} is successfully published");
                        success = true;
                    }
                    catch (Exception e) //TODO: Exception over proxy or enum...
                    {
                        Logger.LogError("Error occured while trying to publish outage.", e);
                    }

                }
            }
            else
            {
                Logger.LogInfo("There is no affected consumers, so reported outage is not valid.");
                success = false;
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
                    if (outageToIsolate.OutageState == OutageState.CREATED)
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
            throw new NotImplementedException();
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
        private List<Consumer> GetAffectedConsumersFromDatabase(List<long> affectedConsumersIds, OutageContext db)
        {
            List<Consumer> affectedConsumers = new List<Consumer>();

            foreach (long affectedConsumerId in affectedConsumersIds)
            {
                Consumer affectedConsumer = db.Consumers.Find(affectedConsumerId);

                if (affectedConsumer == null)
                {
                    break;
                }

                affectedConsumers.Add(affectedConsumer);
            }

            return affectedConsumers;
        }

        private void PublishActiveOutage(Topic topic, OutageMessage outageMessage)
        {
            OutagePublication outagePublication = new OutagePublication(topic, outageMessage);

            using (PublisherProxy publisherProxy = proxyFactory.CreateProxy<PublisherProxy, IPublisher>(EndpointNames.PublisherEndpoint))
            {
                if (publisherProxy == null)
                {
                    string errMsg = "Publisher proxy is null";
                    Logger.LogWarn(errMsg);
                    throw new NullReferenceException(errMsg);
                }

                publisherProxy.Publish(outagePublication);
                Logger.LogInfo($"Outage service published data from topic: {outagePublication.Topic}");
            }
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
                    measuremnts = measurementMapProxy.GetMeasurementsForElement(currentBreakerId);

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

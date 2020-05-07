using OMSCommon.Mappers;
using OMSCommon.OutageDatabaseModel;
using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.OutageService.Interface;
using Outage.Common.OutageService.Model;
using Outage.Common.PubSub;
using Outage.Common.PubSub.CalculationEngineDataContract;
using Outage.Common.PubSub.OutageDataContract;
using Outage.Common.ServiceContracts.CalculationEngine;
using Outage.Common.ServiceContracts.GDA;
using Outage.Common.ServiceContracts.OMS;
using Outage.Common.ServiceContracts.PubSub;
using Outage.Common.ServiceContracts.SCADA;
using Outage.Common.ServiceProxies;
using Outage.Common.ServiceProxies.CalcualtionEngine;
using Outage.Common.ServiceProxies.Outage;
using Outage.Common.ServiceProxies.PubSub;
using OutageDatabase.Repository;
using OutageManagementService.ScadaSubscriber;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using AutoResetEvent = System.Threading.AutoResetEvent;
using OMSCommon.OutageDatabaseModel;
using OutageManagementService.LifeCycleServices;

namespace OutageManagementService
{
    public class CancelationObject
    {
        public bool CancelationSignal { get; set; }
    }

    public sealed class OutageModel : ISubscriberCallback, IDisposable
    {
        private IOutageTopologyModel topologyModel;

        public SwitchOpened SwitchOpened { get; set; }
        public ConsumersBlackedOut ConsumersBlackedOut { get; set; }
        public ConsumersEnergized ConsumersEnergized { get; set; }
        public IOutageTopologyModel TopologyModel
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

        private ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        private ProxyFactory proxyFactory;
        public OutageMessageMapper outageMessageMapper;
        public ModelResourcesDesc modelResourcesDesc;
        public UnitOfWork dbContext { get; private set; }
        private UnitOfWork transactionDbContext;
        private SubscriberProxy subscriberProxy;

        public HashSet<long> commandedElements;
        public HashSet<long> optimumIsolationPoints;
        private Dictionary<DeltaOpType, List<long>> modelChanges;

        private Dictionary<long, Dictionary<long, List<long>>> recloserOutageMap;

        public List<long> CalledOutages;
        public ConcurrentQueue<long> EmailMsg;

        public OutageModel()
        {
            proxyFactory = new ProxyFactory();
            commandedElements = new HashSet<long>();
            optimumIsolationPoints = new HashSet<long>();
            outageMessageMapper = new OutageMessageMapper();
            modelResourcesDesc = new ModelResourcesDesc();
            recloserOutageMap = new Dictionary<long, Dictionary<long, List<long>>>();

            dbContext = new UnitOfWork();

            CalledOutages = new List<long>();
            EmailMsg = new ConcurrentQueue<long>();

            ImportTopologyModel();
            SubscribeToOMSTopologyPublications();
        }

        #region IDisposable
        public void Dispose()
        {
            dbContext.Dispose();
            subscriberProxy.Close();
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

            List<Consumer> consumerDbEntities = transactionDbContext.ConsumerRepository.GetAll().ToList();

            foreach(Consumer consumer in consumerDbEntities)
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

        #region Private Methods
        private string GetDefaultIsolationEndpointsString(long gid, long recloserId)
        {
            string defaultIsolationEndpoints = "";
            if (recloserId != -1)
            {
                defaultIsolationEndpoints = $"{gid}|{recloserId}";
            }
            else
            {
                defaultIsolationEndpoints = $"{gid}";
            }

            return defaultIsolationEndpoints;
        }

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

                TopologyModel = omsTopologyProxy.GetOMSModel();
            }
        }

        private void SubscribeToOMSTopologyPublications()
        {
            Logger.LogDebug("Subcribing on OMS Topology.");
            subscriberProxy = proxyFactory.CreateProxy<SubscriberProxy, ISubscriber>(this, EndpointNames.SubscriberEndpoint);

            if (subscriberProxy == null)
            {
                string message = "SubscribeToOMSTopologyPublications() => SubscriberProxy is null.";
                Logger.LogError(message);
                throw new NullReferenceException(message);
            }

            subscriberProxy.Subscribe(Topic.OMS_MODEL);
        }

        public long GetNextBreaker(long breakerId)
        {
            if (!TopologyModel.OutageTopology.ContainsKey(breakerId))
            {
                string message = $"Breaker with gid: 0x{breakerId:X16} is not in a topology model.";
                Logger.LogError(message);
                throw new Exception(message);
            }

            long nextBreakerId = -1;

            foreach(long elementId in TopologyModel.OutageTopology[breakerId].SecondEnd)
            {
                if (modelResourcesDesc.GetModelCodeFromId(elementId) == ModelCode.ACLINESEGMENT)
                {
                    nextBreakerId = GetNextBreaker(elementId);
                }
                else if (modelResourcesDesc.GetModelCodeFromId(elementId) != ModelCode.BREAKER)
                {
                    return -1;
                }
                else
                {
                    return elementId;
                }

                if(nextBreakerId != -1)
                {
                    break;
                }
            }

            return nextBreakerId;
        }

        public long GetRecloserForHeadBreaker(long headBreakerId)
        {
            long recolserId = -1;

            if (!TopologyModel.OutageTopology.ContainsKey(headBreakerId))
            {
                string message = $"Head switch with gid: {headBreakerId} is not in a topology model.";
                Logger.LogError(message);
                throw new Exception(message);
            }
            long currentBreakerId = headBreakerId;
            while (currentBreakerId != 0)
            {
                //currentBreakerId = TopologyModel.OutageTopology[currentBreakerId].SecondEnd.Where(element => modelResourcesDesc.GetModelCodeFromId(element) == ModelCode.BREAKER).FirstOrDefault();
                currentBreakerId = GetNextBreaker(currentBreakerId);
                if (currentBreakerId == 0)
                {
                    continue;
                }

                if (!TopologyModel.OutageTopology.ContainsKey(currentBreakerId))
                {
                    string message = $"Switch with gid: 0X{currentBreakerId:X16} is not in a topology model.";
                    Logger.LogError(message);
                    throw new Exception(message);
                }

                if (!TopologyModel.OutageTopology[currentBreakerId].NoReclosing)
                {
                    recolserId = currentBreakerId;
                    break;
                }
            }

            return recolserId;
        }

        public List<Consumer> GetAffectedConsumersFromDatabase(List<long> affectedConsumersIds)
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

        public List<Equipment> GetEquipmentEntity(List<long> equipmentIds)
        {
            List<long> equipementIdsNotFoundInDb = new List<long>();
            List<Equipment> equipmentList = new List<Equipment>();

            foreach (long equipmentId in equipmentIds)
            {
                Equipment equipmentDbEntity = dbContext.EquipmentRepository.Get(equipmentId);

                if (equipmentDbEntity == null)
                {
                    equipementIdsNotFoundInDb.Add(equipmentId);
                }
                else
                {
                    equipmentList.Add(equipmentDbEntity);
                }
            }

            equipmentList.AddRange(CreateEquipementEntitiesFromNmsData(equipementIdsNotFoundInDb));

            return equipmentList;
        }

        public List<Equipment> CreateEquipementEntitiesFromNmsData(List<long> entityIds)
        {
            List<Equipment> equipements = new List<Equipment>();

            List<ModelCode> propIds = new List<ModelCode>() { ModelCode.IDOBJ_MRID };

            using(NetworkModelGDAProxy proxy = proxyFactory.CreateProxy<NetworkModelGDAProxy, INetworkModelGDAContract>(EndpointNames.NetworkModelGDAEndpoint))
            {
                if (proxy == null)
                {
                    string message = "OutageModel::CreateEquipementEntitiesFromNmsData => NetworkModelGDAProxy is null";
                    Logger.LogError(message);
                    throw new NullReferenceException();
                }

                foreach(long gid in entityIds)
                {
                    ResourceDescription rd = null;

                    try
                    {
                        rd = proxy.GetValues(gid, propIds);
                    }
                    catch (Exception e)
                    {
                        //TODO: Kad prvi put ovde bude puklo, alarmirajte me. Dimitrije
                        throw e;
                    }

                    if(rd == null)
                    {
                        continue;
                    }

                    Equipment createdEquipement = new Equipment()
                    {
                        EquipmentId = rd.Id,
                        EquipmentMRID = rd.Properties[0].AsString(),
                    };

                    equipements.Add(createdEquipement);
                }
            }

            return equipements;
        }

        public bool PublishOutage(Topic topic, OutageMessage outageMessage)
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

        [Obsolete]
        private List<long> ParseIsolationPointsFromCSV(string isolationPointsCSV)
        {
            List<long> isolationPoints = new List<long>();

            foreach(string isolationPointString in isolationPointsCSV.Split('|'))
            {
                if(long.TryParse(isolationPointString, out long isolationPointId))
                {
                    isolationPoints.Add(isolationPointId);
                }
                else
                {
                    Logger.LogError("Parsing error in ParseIsolationPointsFromCSV.");
                    isolationPoints.Clear();
                    //throw?
                }
            }

            return isolationPoints;
        }

        [Obsolete]
        private string ParseIsolationPointsToCSV(List<long> isolationPoints)
        {
            StringBuilder isolationPointsCSV = new StringBuilder();

            for (int i = 0; i < isolationPoints.Count; i++)
            {
                isolationPointsCSV.Append($"{isolationPoints[i]}");

                if(i < isolationPoints.Count - 1)
                {
                    isolationPointsCSV.Append("|");
                }
            }

            return isolationPointsCSV.ToString();
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

        #region ISubscriberCallback
        public string GetSubscriberName()
        {
            return "OUTAGE_MODEL_SUBSCRIBER";
        }

        public void Notify(IPublishableMessage message)
        {
            if(message is OMSModelMessage omsModelMessage)
            {
                TopologyModel = omsModelMessage.OutageTopologyModel; //TODO: Da li su subsciber callback pozivi sinhroni?
                HashSet<long> energizedConsumers = new HashSet<long>();
                foreach (var element in TopologyModel.OutageTopology.Values)
                {
                    if (element.DmsType.Equals(DMSType.ENERGYCONSUMER.ToString()))
                    {
                        if (element.IsActive)
                        {
                            energizedConsumers.Add(element.Id);
                        }
                    }
                }
                ConsumersEnergized?.Invoke(energizedConsumers);
            }
            else
            {
                Logger.LogWarn("OutageModel::Notify => UNKNOWN message type. OMSModelMessage expected.");
            }
        }

        public void CheckForClosedBreakers(IOutageTopologyModel outageTopologyModel)
        {
            foreach (var element in outageTopologyModel.OutageTopology)
            {
                if(element.Value.DmsType == "BREAKER")
                {
                    //TODO: dobiti od CE info da li su breakere Opened ili ne
                }
            }
        }
        #endregion
    }
}

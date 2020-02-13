using CECommon.Interfaces;
using CECommon.Model;
using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.OutageService.Interface;
using Outage.Common.OutageService.Model;
using Outage.Common.PubSub.OutageDataContract;
using Outage.Common.ServiceContracts.CalculationEngine;
using Outage.Common.ServiceContracts.GDA;
using Outage.Common.ServiceContracts.OMS;
using Outage.Common.ServiceContracts.PubSub;
using Outage.Common.ServiceProxies;
using Outage.Common.ServiceProxies.CalcualtionEngine;
using Outage.Common.ServiceProxies.PubSub;
using Outage.Common.UI;
using OutageDatabase;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TopologyServiceClientMock;

namespace OutageManagementService
{
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

        public bool ReportPotentialOutage(long gid)
        {
            bool success = false;

            List<long> affectedConsumersIds = new List<long>();

            //TODO: special case: potenitial outage is remote (and closed)...

            affectedConsumersIds = GetAffectedConsumers(gid);

            if (affectedConsumersIds.Count > 0)
            {
                ActiveOutage activeOutageInDb = null;
                using (OutageContext db = new OutageContext())
                {
                    try
                    {
                        List<Consumer> consumers = GetAffectedConsumersFromDatabase(affectedConsumersIds, db);
                        if(consumers.Count == affectedConsumersIds.Count)
                        {
                            activeOutageInDb = db.ActiveOutages.Add(new ActiveOutage { AffectedConsumers = consumers, ElementGid = gid, ReportTime = DateTime.Now });
                            db.SaveChanges();
                            Logger.LogDebug($"Outage on element with gid: 0x{activeOutageInDb.ElementGid:x16} is successfully stored in database");
                        }
                        else
                        {
                            Logger.LogWarn("Some of affected consumers are not present in database.");
                            success = false;
                        }
                        
                    }
                    catch (Exception e)
                    {
                        Logger.LogError("Error while adding active outage into database.", e);
                        success = false;
                    }
                }
                //TODO: Publish
                if (activeOutageInDb != null)
                {
                    try
                    {
                        PublishActiveOutage(Topic.ACTIVE_OUTAGE, activeOutageInDb);
                        Logger.LogInfo($"Outage on element with gid: 0x{activeOutageInDb.ElementGid:x16} is successfully reported");
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

        private List<Consumer> GetAffectedConsumersFromDatabase(List<long> affectedConsumersIds, OutageContext db)
        {
            List<Consumer> affectedConsumers = new List<Consumer>();
            
            foreach(long affectedConsumerId in affectedConsumersIds)
            {
                Consumer affectedConsumer = db.Consumers.Find(affectedConsumerId);

                if(affectedConsumer == null)
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

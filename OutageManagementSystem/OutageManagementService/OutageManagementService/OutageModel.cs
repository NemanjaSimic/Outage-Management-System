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

        //private PublisherProxy publisherProxy = null;

        //private PublisherProxy GetPublisherProxy()
        //{
        //    //TODO: diskusija statefull vs stateless

        //    int numberOfTries = 0;
        //    int sleepInterval = 500;

        //    while (numberOfTries <= int.MaxValue)
        //    {
        //        try
        //        {
        //            if (publisherProxy != null)
        //            {
        //                publisherProxy.Abort();
        //                publisherProxy = null;
        //            }

        //            publisherProxy = new PublisherProxy(EndpointNames.PublisherEndpoint);
        //            publisherProxy.Open();

        //            if (publisherProxy.State == CommunicationState.Opened)
        //            {
        //                break;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            string message = $"Exception on PublisherProxy initialization. Message: {ex.Message}";
        //            Logger.LogError(message, ex);
        //            publisherProxy = null;
        //        }
        //        finally
        //        {
        //            numberOfTries++;
        //            Logger.LogDebug($"OutageModel: PublisherProxy getter, try number: {numberOfTries}.");

        //            if (numberOfTries >= 100)
        //            {
        //                sleepInterval = 1000;
        //            }

        //            Thread.Sleep(sleepInterval);
        //        }
        //    }

        //    return publisherProxy;
        //}

        //#region Proxies
        //private OMSTopologyServiceProxy omsTopologyServiceProxy = null;

        //private OMSTopologyServiceProxy GetTopologyProxy()
        //{
        //    int numberOfTries = 0;
        //    int sleepInterval = 500;

        //    while (numberOfTries <= int.MaxValue)
        //    {
        //        try
        //        {
        //            if (omsTopologyServiceProxy != null)
        //            {
        //                omsTopologyServiceProxy.Abort();
        //                omsTopologyServiceProxy = null;
        //            }

        //            omsTopologyServiceProxy = new OMSTopologyServiceProxy(EndpointNames.TopologyOMSServiceEndpoint);
        //            omsTopologyServiceProxy.Open();

        //            if (omsTopologyServiceProxy.State == CommunicationState.Opened)
        //            {
        //                break;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            string message = $"Exception on OMSTopologyServiceProxy initialization. Message: {ex.Message}";
        //            Logger.LogWarn(message, ex);
        //            omsTopologyServiceProxy = null;
        //        }
        //        finally
        //        {
        //            numberOfTries++;
        //            Logger.LogDebug($"OutageModel: OMSTopologyServiceProxy getter, try number: {numberOfTries}.");

        //            if (numberOfTries >= 100)
        //            {
        //                sleepInterval = 1000;
        //            }

        //            Thread.Sleep(sleepInterval);
        //        }
        //    }

        //    return omsTopologyServiceProxy;
        //}

        //private NetworkModelGDAProxy gdaQueryProxy = null;

        //private NetworkModelGDAProxy GetGdaQueryProxy()
        //{
        //    int numberOfTries = 0;
        //    int sleepInterval = 500;

        //    while (numberOfTries <= int.MaxValue)
        //    {
        //        try
        //        {
        //            if (gdaQueryProxy != null)
        //            {
        //                gdaQueryProxy.Abort();
        //                gdaQueryProxy = null;
        //            }

        //            gdaQueryProxy = new NetworkModelGDAProxy(EndpointNames.NetworkModelGDAEndpoint);
        //            gdaQueryProxy.Open();

        //            if (gdaQueryProxy.State == CommunicationState.Opened)
        //            {
        //                break;
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            string message = $"Exception on NetworkModelGDAProxy initialization. Message: {ex.Message}";
        //            Logger.LogWarn(message, ex);
        //            gdaQueryProxy = null;
        //        }
        //        finally
        //        {
        //            numberOfTries++;
        //            Logger.LogDebug($"NetworkModelGDA: GdaQueryProxy getter, try number: {numberOfTries}.");

        //            if (numberOfTries >= 100)
        //            {
        //                sleepInterval = 1000;
        //            }

        //            Thread.Sleep(sleepInterval);
        //        }
        //    }

        //    return gdaQueryProxy;
        //}
        //#endregion

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
                if (omsTopologyProxy != null)
                {
                    TopologyModel = (OutageTopologyModel)omsTopologyProxy.GetOMSModel();
                }
                else
                {
                    string message = "From method ImportTopologyModel(): TopologyServiceProxy is null.";
                    logger.LogError(message);
                    throw new NullReferenceException(message);
                }
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

                if(affectedConsumer != null)
                {
                    affectedConsumers.Add(affectedConsumer);
                }
                else
                {
                    break;
                }
            }

            return affectedConsumers;
        }

        private void PublishActiveOutage(Topic topic, OutageMessage outageMessage)
        {
            OutagePublication outagePublication = new OutagePublication(topic, outageMessage);

            using (PublisherProxy publisherProxy = proxyFactory.CreateProxy<PublisherProxy, IPublisher>(EndpointNames.PublisherEndpoint))
            {
                if (publisherProxy != null)
                {
                    publisherProxy.Publish(outagePublication);
                    Logger.LogInfo($"Outage service published data from topic: {outagePublication.Topic}");
                }
                else
                {
                    string errMsg = "Publisher proxy is null";
                    Logger.LogWarn(errMsg);
                    throw new NullReferenceException(errMsg);
                }
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
                    IOutageTopologyElement topologyElement = topologyModel.OutageTopology[currentNode];

                    if (topologyElement.SecondEnd.Count == 0 && topologyElement.DmsType == "ENERGYCONSUMER") 
                    {
                        affectedConsumers.Add(currentNode);
                    }

                    foreach(long adjNode in topologyElement.SecondEnd)
                    {
                        nodesToBeVisited.Push(adjNode);
                    }
                }
            }

            return affectedConsumers;
        }

        #region GDAHelper
        private Dictionary<long, ResourceDescription> GetExtentValues(ModelCode entityType, List<ModelCode> propIds)
        {
            int iteratorId = 0;
            int numberOfTries = 0;
            while (numberOfTries < 5)
            {
                try
                {
                    numberOfTries++;
                    using (NetworkModelGDAProxy proxy = proxyFactory.CreateProxy<NetworkModelGDAProxy, INetworkModelGDAContract>(EndpointNames.NetworkModelGDAEndpoint))
                    {
                        iteratorId = proxy.GetExtentValues(entityType, propIds);
                    }
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError($"Failed to get extent values for entity type {entityType.ToString()}. Exception message: " + ex.Message);
                    logger.LogWarn($"Retrying to connect to NMSProxy. Number of tries: {numberOfTries}.");
                }
            }

            return ProcessIterator(iteratorId);
        }

        private Dictionary<long, ResourceDescription> ProcessIterator(int iteratorId)
        {
            //TODO: mozda vec ovde napakovati dictionary<long, rd> ?
            int numberOfResources = 10000, resourcesLeft = 0;
            Dictionary<long, ResourceDescription> resourceDescriptions = new Dictionary<long, ResourceDescription>();

            try
            {
                using (NetworkModelGDAProxy gdaProxy = proxyFactory.CreateProxy<NetworkModelGDAProxy, INetworkModelGDAContract>(EndpointNames.NetworkModelGDAEndpoint))
                {
                    if (gdaProxy != null)
                    {
                        do
                        {
                            List<ResourceDescription> rds = gdaProxy.IteratorNext(numberOfResources, iteratorId);
                            foreach(var rd in rds)
                            {
                                resourceDescriptions.Add(rd.Id, rd);
                            }

                            resourcesLeft = gdaProxy.IteratorResourcesLeft(iteratorId);

                        } while (resourcesLeft > 0);

                        gdaProxy.IteratorClose(iteratorId);
                    }
                    else
                    {
                        string message = "From method ProcessIterator(): NetworkModelGDAProxy is null.";
                        logger.LogError(message);
                        throw new NullReferenceException(message);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = $"Failed to retrieve all Resourse descriptions with iterator {iteratorId}. Exception message: " + ex.Message;
                logger.LogError(message);
            }
            return resourceDescriptions;
        }
        #endregion
    }
}

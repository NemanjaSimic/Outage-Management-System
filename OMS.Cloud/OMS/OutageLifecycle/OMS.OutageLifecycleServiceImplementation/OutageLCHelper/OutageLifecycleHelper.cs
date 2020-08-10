using Common.OMS.OutageDatabaseModel;
using Common.PubSubContracts.DataContracts.OMS;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.NmsContracts;
using OMS.Common.NmsContracts.GDA;
using OMS.Common.PubSub;
using OMS.Common.PubSubContracts;
using OMS.Common.WcfClient.NMS;
using OMS.Common.WcfClient.PubSub;
using OutageDatabase.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.OutageLifecycleServiceImplementation.OutageLCHelper
{
	public class OutageLifecycleHelper
    {
        private UnitOfWork dbContext;
        private IOutageTopologyModel outageTopology;
        private ICloudLogger logger;
        public static ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();
        private INetworkModelGDAContract networkModelGdaClient;
        private IPublisherContract publisherClient;
        
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }
        public OutageLifecycleHelper(UnitOfWork unitOfWork, IOutageTopologyModel outageTopology)
        {
            this.dbContext = unitOfWork;
            this.outageTopology = outageTopology;
            this.networkModelGdaClient = NetworkModelGdaClient.CreateClient();
            this.publisherClient = PublisherClient.CreateClient();
        }

        public List<long> GetAffectedConsumers(long potentialOutageGid)
        {
            List<long> affectedConsumers = new List<long>();
            Stack<long> nodesToBeVisited = new Stack<long>();
            HashSet<long> visited = new HashSet<long>();
            long startingSwitch = potentialOutageGid;

            if (this.outageTopology.OutageTopology.TryGetValue(potentialOutageGid, out IOutageTopologyElement firstElement)
                && this.outageTopology.OutageTopology.TryGetValue(firstElement.FirstEnd, out IOutageTopologyElement currentElementAbove))
            {
                while (!currentElementAbove.DmsType.Equals("ENERGYSOURCE"))
                {
                    if (currentElementAbove.IsOpen)
                    {
                        startingSwitch = currentElementAbove.Id;
                        break;
                    }

                    if (!this.outageTopology.OutageTopology.TryGetValue(currentElementAbove.FirstEnd, out currentElementAbove))
                    {
                        break;
                    }
                }
            }

            nodesToBeVisited.Push(startingSwitch);

            while (nodesToBeVisited.Count > 0)
            {
                long currentNode = nodesToBeVisited.Pop();

                if (!visited.Contains(currentNode))
                {
                    visited.Add(currentNode);

                    if (this.outageTopology.OutageTopology.TryGetValue(currentNode, out IOutageTopologyElement topologyElement))
                    {
                        if (topologyElement.DmsType == "ENERGYCONSUMER" && !topologyElement.IsActive)
                        {
                            affectedConsumers.Add(currentNode);
                        }
                        else if (topologyElement.DmsType == "ENERGYCONSUMER" && !topologyElement.IsRemote)
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
        public List<Consumer> GetAffectedConsumersFromDatabase(List<long> affectedConsumersIds)
        {
            List<Consumer> affectedConsumers = new List<Consumer>();

            foreach (long affectedConsumerId in affectedConsumersIds)
            {
                Consumer affectedConsumer = this.dbContext.ConsumerRepository.Get(affectedConsumerId);

                if (affectedConsumer == null)
                {
                    break;
                }

                affectedConsumers.Add(affectedConsumer);
            }

            return affectedConsumers;
        }
        public long GetRecloserForHeadBreaker(long headBreakerId)
        {
            long recolserId = -1;

            if (!this.outageTopology.OutageTopology.ContainsKey(headBreakerId))
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

                if (!outageTopology.OutageTopology.ContainsKey(currentBreakerId))
                {
                    string message = $"Switch with gid: 0X{currentBreakerId:X16} is not in a topology model.";
                    Logger.LogError(message);
                    throw new Exception(message);
                }

                if (!outageTopology.OutageTopology[currentBreakerId].NoReclosing)
                {
                    recolserId = currentBreakerId;
                    break;
                }
            }

            return recolserId;
        }
        public long GetNextBreaker(long breakerId)
        {
            if (!outageTopology.OutageTopology.ContainsKey(breakerId))
            {
                string message = $"Breaker with gid: 0x{breakerId:X16} is not in a topology model.";
                Logger.LogError(message);
                throw new Exception(message);
            }

            long nextBreakerId = -1;

            foreach (long elementId in outageTopology.OutageTopology[breakerId].SecondEnd)
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

                if (nextBreakerId != -1)
                {
                    break;
                }
            }

            return nextBreakerId;
        }

        public async Task<List<Equipment>> GetEquipmentEntity(List<long> equipmentIds)
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

            equipmentList.AddRange(await CreateEquipementEntitiesFromNmsData(equipementIdsNotFoundInDb));

            return equipmentList;
        }

        public async Task<List<Equipment>> CreateEquipementEntitiesFromNmsData(List<long> entityIds)
        {
            List<Equipment> equipements = new List<Equipment>();

            List<ModelCode> propIds = new List<ModelCode>() { ModelCode.IDOBJ_MRID };


            if (networkModelGdaClient == null)
            {
                string message = "OutageModel::CreateEquipementEntitiesFromNmsData => NetworkModelGDAProxy is null";
                Logger.LogError(message);
                throw new NullReferenceException();
            }

            foreach (long gid in entityIds)
            {
                ResourceDescription rd = null;

                try
                {
                    rd = await networkModelGdaClient.GetValues(gid, propIds);
                }
                catch (Exception e)
                {
                    //TODO: Kad prvi put ovde bude puklo, alarmirajte me. Dimitrije
                    throw e;
                }

                if (rd == null)
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
            

            return equipements;
        }

        public async Task<bool> PublishOutage(Topic topic, OutageMessage outageMessage)
        {
            bool success;

            OutagePublication outagePublication = new OutagePublication(topic, outageMessage);

         
                if (publisherClient == null)
                {
                    string errMsg = "Publisher proxy is null";
                    Logger.LogWarning(errMsg);
                    throw new NullReferenceException(errMsg);
                }

                try
                {
                    await publisherClient.Publish(outagePublication, "OutagePublisher"); //TODO: Service defines
                    Logger.LogWarning($"Outage service published data from topic: {outagePublication.Topic}");
                    success = true;
                }
                catch (Exception e)
                {
                    string message = $"OutageModel::PublishActiveOutage => exception on PublisherProxy.Publish()";
                    Logger.LogError(message, e);
                    success = false;
                }
            

            return success;
        }
    }
}

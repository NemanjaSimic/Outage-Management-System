using Common.OmsContracts.DataContracts.OutageDatabaseModel;
using Common.PubSubContracts.DataContracts.CE;
using Common.PubSubContracts.DataContracts.OMS;
using Microsoft.ServiceFabric.Data;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Names;
using OMS.Common.NmsContracts;
using OMS.Common.NmsContracts.GDA;
using OMS.Common.WcfClient.CE;
using OMS.Common.WcfClient.NMS;
using OMS.Common.WcfClient.OMS.ModelAccess;
using OMS.Common.WcfClient.OMS.ModelProvider;
using OMS.Common.WcfClient.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OMS.OutageLifecycleImplementation.Helpers
{
    public class OutageLifecycleHelper
    {
        private readonly string baseLogString;
        private readonly ModelResourcesDesc modelResourcesDesc;

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        public OutageLifecycleHelper(ModelResourcesDesc modelResourcesDesc)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            this.modelResourcesDesc = modelResourcesDesc;
        }

        #region Consumer Helpers
        public List<long> GetAffectedConsumers(long potentialOutageGid, OutageTopologyModel topology)
        {
            List<long> affectedConsumers = new List<long>();
            Stack<long> nodesToBeVisited = new Stack<long>();
            HashSet<long> visited = new HashSet<long>();
            long startingSwitch = potentialOutageGid;

            if (topology.OutageTopology.TryGetValue(potentialOutageGid, out OutageTopologyElement firstElement)
                && topology.OutageTopology.TryGetValue(firstElement.FirstEnd, out OutageTopologyElement currentElementAbove))
            {
                while (!currentElementAbove.DmsType.Equals("ENERGYSOURCE"))
                {
                    if (currentElementAbove.IsOpen)
                    {
                        startingSwitch = currentElementAbove.Id;
                        break;
                    }

                    if (!topology.OutageTopology.TryGetValue(currentElementAbove.FirstEnd, out currentElementAbove))
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

                    if (topology.OutageTopology.TryGetValue(currentNode, out OutageTopologyElement topologyElement))
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
            var consumerAccessClient = ConsumerAccessClient.CreateClient();
            List<Consumer> affectedConsumers = new List<Consumer>();

            foreach (long affectedConsumerId in affectedConsumersIds)
            {
                Consumer affectedConsumer = consumerAccessClient.GetConsumer(affectedConsumerId).Result;

                if (affectedConsumer == null)
                {
                    break;
                }

                affectedConsumers.Add(affectedConsumer);
            }

            return affectedConsumers;
        }
        #endregion Consumer Helpers

        #region Breaker Helpers
        public long GetNextBreaker(long breakerId, OutageTopologyModel topology)
        {
            if (!topology.OutageTopology.ContainsKey(breakerId))
            {
                string message = $"Breaker with gid: 0x{breakerId:X16} is not in a topology model.";
                Logger.LogError(message);
                throw new Exception(message);
            }

            long nextBreakerId = -1;

            foreach (long elementId in topology.OutageTopology[breakerId].SecondEnd)
            {
                if (this.modelResourcesDesc.GetModelCodeFromId(elementId) == ModelCode.ACLINESEGMENT)
                {
                    nextBreakerId = GetNextBreaker(elementId, topology);
                }
                else if (this.modelResourcesDesc.GetModelCodeFromId(elementId) != ModelCode.BREAKER)
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

        public long GetRecloserForHeadBreaker(long headBreakerId, OutageTopologyModel topology)
        {
            long recolserId = -1;

            if (!topology.OutageTopology.ContainsKey(headBreakerId))
            {
                string message = $"Head switch with gid: {headBreakerId} is not in a topology model.";
                Logger.LogError(message);
                throw new Exception(message);
            }
            long currentBreakerId = headBreakerId;
            while (currentBreakerId != 0)
            {
                //currentBreakerId = TopologyModel.OutageTopology[currentBreakerId].SecondEnd.Where(element => modelResourcesDesc.GetModelCodeFromId(element) == ModelCode.BREAKER).FirstOrDefault();
                currentBreakerId = GetNextBreaker(currentBreakerId, topology);
                if (currentBreakerId == 0)
                {
                    continue;
                }

                if (!topology.OutageTopology.ContainsKey(currentBreakerId))
                {
                    string message = $"Switch with gid: 0X{currentBreakerId:X16} is not in a topology model.";
                    Logger.LogError(message);
                    throw new Exception(message);
                }

                if (!topology.OutageTopology[currentBreakerId].NoReclosing)
                {
                    recolserId = currentBreakerId;
                    break;
                }
            }

            return recolserId;
        }

        public async Task<bool> CheckIfBreakerIsRecloserAsync(long elementId)
        {
            bool isRecloser = false;

            try
            {
                var networkModelGdaClient = NetworkModelGdaClient.CreateClient();
                ResourceDescription resourceDescription = await networkModelGdaClient.GetValues(elementId, new List<ModelCode>() { ModelCode.BREAKER_NORECLOSING });
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
            catch (Exception e)
            {
                //todo: log
                throw e;
            }

            return isRecloser;
        }

        public async Task<long> GetHeadBreakerAsync(List<long> defaultIsolationPoints, bool isFirstBreakerRecloser)
        {
            long headBreaker = -1;
            if (defaultIsolationPoints.Count == 2)
            {
                if (isFirstBreakerRecloser)
                {
                    headBreaker = defaultIsolationPoints[1];
                }
                else
                {
                    headBreaker = defaultIsolationPoints[0];
                }

                var outageModelUpdateAccessClient = OutageModelUpdateAccessClient.CreateClient();
                await outageModelUpdateAccessClient.UpdateCommandedElements(headBreaker, ModelUpdateOperationType.INSERT);
            }
            else
            {
                if (!isFirstBreakerRecloser)
                {
                    headBreaker = defaultIsolationPoints[0];
                    var outageModelUpdateAccessClient = OutageModelUpdateAccessClient.CreateClient();
                    await outageModelUpdateAccessClient.UpdateCommandedElements(headBreaker, ModelUpdateOperationType.INSERT);
                }
                else
                {
                    Logger.LogWarning($"Invalid state: breaker with id 0x{defaultIsolationPoints[0]:X16} is the only default isolation element, but it is also a recloser.");
                }
            }

            return headBreaker;
        }

        public async Task<long> GetRecloserAsync(List<long> defaultIsolationPoints, bool isFirstBreakerRecloser)
        {
            long recloser = -1;

            if (defaultIsolationPoints.Count == 2)
            {
                if (isFirstBreakerRecloser)
                {
                    recloser = defaultIsolationPoints[0];
                }
                else
                {
                    recloser = defaultIsolationPoints[1];
                }

                var outageModelUpdateAccessClient = OutageModelUpdateAccessClient.CreateClient();
                await outageModelUpdateAccessClient.UpdateCommandedElements(recloser, ModelUpdateOperationType.INSERT);
            }

            return recloser;
        }
        #endregion Breaker Helpers

        #region Equipment Helpers
        public async Task<List<Equipment>> GetEquipmentEntityAsync(List<long> equipmentIds)
        {
            var equipmentAccessClient = EquipmentAccessClient.CreateClient();
            List<long> equipmentIdsNotFoundInDb = new List<long>();
            List<Equipment> equipmentList = new List<Equipment>();

            foreach (long equipmentId in equipmentIds)
            {
                Equipment equipmentDbEntity = equipmentAccessClient.GetEquipment(equipmentId).Result;

                if (equipmentDbEntity == null)
                {
                    equipmentIdsNotFoundInDb.Add(equipmentId);
                }
                else
                {
                    equipmentList.Add(equipmentDbEntity);
                }
            }

            equipmentList.AddRange(await CreateEquipmentEntitiesFromNmsDataAsync(equipmentIdsNotFoundInDb));

            return equipmentList;
        }

        public async Task<List<Equipment>> CreateEquipmentEntitiesFromNmsDataAsync(List<long> entityIds)
        {
            var networkModelGdaClient = NetworkModelGdaClient.CreateClient();
            List<Equipment> equipments = new List<Equipment>();
            List<ModelCode> propIds = new List<ModelCode>() { ModelCode.IDOBJ_MRID };

            foreach (long gid in entityIds)
            {
                ResourceDescription rd = null;

                try
                {
                    rd = await networkModelGdaClient.GetValues(gid, propIds);
                }
                catch (Exception e)
                {
                    //todo: log
                    //TODO: Kad prvi put ovde bude puklo, alarmirajte me. Dimitrije
                    throw e;
                }

                if (rd == null)
                {
                    continue;
                }

                Equipment createdEquipment = new Equipment()
                {
                    EquipmentId = rd.Id,
                    EquipmentMRID = rd.Properties[0].AsString(),
                };

                equipments.Add(createdEquipment);
            }
            

            return equipments;
        }
        #endregion Equipment Helpers

        #region Commanding Helpers
        public async Task SendScadaCommandAsync(long breakerGid, DiscreteCommandingType discreteCommandingType)
        {
            var measurementMapClient = MeasurementMapClient.CreateClient();
            List<long> measurements = await measurementMapClient.GetMeasurementsOfElement(breakerGid);

            if (measurements.Count == 0)
            {
                Logger.LogWarning($"{baseLogString} SendSCADACommand => Element with gid: 0x{breakerGid:X16} has no measurements.");
                return;
            }

            var measurement = measurements.FirstOrDefault();

            if (measurement == 0)
            {
                Logger.LogWarning($"{baseLogString} SendSCADACommand => Measurement gid is 0.");
                return;
            }

            var outageModelReadAccessClient = OutageModelReadAccessClient.CreateClient();
            var commandedElements = await outageModelReadAccessClient.GetCommandedElements();

            if (discreteCommandingType == DiscreteCommandingType.OPEN && !commandedElements.ContainsKey(breakerGid))
            {
                var outageModelUpdateAccessClient = OutageModelUpdateAccessClient.CreateClient();
                await outageModelUpdateAccessClient.UpdateCommandedElements(breakerGid, ModelUpdateOperationType.INSERT);
            }

            //todo: SendOpenCommand da vraca bool pa onda provera
            var switchStatusCommandingClient = SwitchStatusCommandingClient.CreateClient();
            await switchStatusCommandingClient.SendOpenCommand(measurement);

            //todo: u vezi gornjeg todo
            //if(fail)
            //{
            //    if (discreteCommandingType == DiscreteCommandingType.OPEN && commandedElements.ContainsKey(currentBreakerId))
            //    {
            //        var outageModelUpdateAccessClient = OutageModelUpdateAccessClient.CreateClient();
            //        await outageModelUpdateAccessClient.UpdateCommandedElements(currentBreakerId, ModelUpdateOperationType.DELETE);
            //    }
            //}
        }
        #endregion Commanding Helpers

        #region Publishing Helpers
        public async Task<bool> PublishOutageAsync(Topic topic, OutageMessage outageMessage)
        {
            bool success;

            try
            {
                OutagePublication outagePublication = new OutagePublication(topic, outageMessage);

                var publisherClient = PublisherClient.CreateClient();
                await publisherClient.Publish(outagePublication, MicroserviceNames.OmsOutageLifecycleService);
                Logger.LogInformation($"{baseLogString} PublishOutage => Outage service published data to topic: {outagePublication.Topic}");

                success = true;
            }
            catch (Exception e)
            {
                string message = $"{baseLogString} PublishOutage => exception: {e.Message}";
                Logger.LogError(message, e);

                success = false;
            }

            return success;
        }
        #endregion Publishing Helpers

        #region Outage Helpers
        public async Task<ConditionalValue<OutageEntity>> GetOutageEntity(long outageId)
        {
            var outageModelAccessClient = OutageModelAccessClient.CreateClient();
            var outageDbEntity = await outageModelAccessClient.GetOutage(outageId);

            if (outageDbEntity == null)
            {
                Logger.LogError($"{baseLogString} GetOutageEntity => Outage with id 0x{outageId:X16} is not found in database.");
                return new ConditionalValue<OutageEntity>(false, null);
            }

            return new ConditionalValue<OutageEntity>(true, outageDbEntity);
        }

        public async Task<ConditionalValue<OutageEntity>> GetCreatedOutage(long outageId)
        {
            var result = await GetOutageEntity(outageId);

            if (!result.HasValue)
            {
                return new ConditionalValue<OutageEntity>(false, null);
            }

            var outageEntity = result.Value;

            if (outageEntity.OutageState != OutageState.CREATED)
            {
                Logger.LogError($"{baseLogString} GetCreatedOutage => Outage with id 0x{outageId:X16} is in state {outageEntity.OutageState}, and thus repair crew can not be sent. (Expected state: {OutageState.ISOLATED})");
                return new ConditionalValue<OutageEntity>(false, null);
            }

            return new ConditionalValue<OutageEntity>(true, outageEntity);
        }

        public async Task<ConditionalValue<OutageEntity>> GetIsolatedOutage(long outageId)
        {
            var result = await GetOutageEntity(outageId);

            if (!result.HasValue)
            {
                return new ConditionalValue<OutageEntity>(false, null);
            }

            var outageEntity = result.Value;

            if (outageEntity.OutageState != OutageState.ISOLATED)
            {
                Logger.LogError($"{baseLogString} GetIsolatedOutage => Outage with id 0x{outageId:X16} is in state {outageEntity.OutageState}, and thus repair crew can not be sent. (Expected state: {OutageState.ISOLATED})");
                return new ConditionalValue<OutageEntity>(false, null);
            }

            return new ConditionalValue<OutageEntity>(true, outageEntity);
        }

        public async Task<ConditionalValue<OutageEntity>> GetRepairedOutage(long outageId)
        {
            var result = await GetOutageEntity(outageId);

            if (!result.HasValue)
            {
                return new ConditionalValue<OutageEntity>(false, null);
            }

            var outageEntity = result.Value;

            if (outageEntity.OutageState != OutageState.REPAIRED)
            {
                Logger.LogError($"{baseLogString} GetRepairedOutage => Outage with id 0x{outageId:X16} is in state {outageEntity.OutageState}, and thus repair crew can not be sent. (Expected state: {OutageState.REPAIRED})");
                return new ConditionalValue<OutageEntity>(false, null);
            }

            return new ConditionalValue<OutageEntity>(true, outageEntity);
        }

        public async Task<ConditionalValue<OutageEntity>> GetValidatedRepairedOutage(long outageId)
        {
            var result = await GetRepairedOutage(outageId);

            if (!result.HasValue)
            {
                return new ConditionalValue<OutageEntity>(false, null);
            }

            var outageDbEntity = result.Value;

            if (!outageDbEntity.IsResolveConditionValidated)
            {
                Logger.LogError($"{baseLogString} GetValidatedRepairedOutage => resolve conditions not validated.");
                return new ConditionalValue<OutageEntity>(false, null);
            }

            return new ConditionalValue<OutageEntity>(true, outageDbEntity);
        }
        #endregion Outage Helpers
    }
}

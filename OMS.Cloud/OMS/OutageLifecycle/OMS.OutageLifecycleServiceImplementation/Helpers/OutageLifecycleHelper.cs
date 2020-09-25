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

            if (equipmentIdsNotFoundInDb.Count > 0)
			{
                List<Equipment> createdEquipments = await CreateEquipmentEntitiesFromNmsDataAsync(equipmentIdsNotFoundInDb);

                var equipmentAccess = EquipmentAccessClient.CreateClient();

                List<Task> addingEquipmentTasks = new List<Task>();
                foreach (var createdEquipment in createdEquipments)
                {
                    addingEquipmentTasks.Add(Task.Run(() => equipmentAccess.AddEquipment(createdEquipment)));
                }

                Task.WaitAll(addingEquipmentTasks.ToArray());
                equipmentList.AddRange(createdEquipments);
            }

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
                    Logger.LogError($"{baseLogString} CreateEquipmentEntitiesFromNmsDataAsync => Exception: {e.Message}");
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
        public async Task<bool> SendSingleScadaCommandAsync(long breakerGid, DiscreteCommandingType discreteCommandingType, Dictionary<long, CommandedElement> commandedElements, CommandOriginType commandOriginType)
        {
            if (commandedElements.ContainsKey(breakerGid) && commandedElements[breakerGid].CommandingType == discreteCommandingType)
            {
                Logger.LogDebug($"{baseLogString} SendSingleScadaCommandAsync => Trying to send duplicate command. Aborting call.");
                return false;
            }

            var measurementMapClient = MeasurementMapClient.CreateClient();
            List<long> measurementGids = await measurementMapClient.GetMeasurementsOfElement(breakerGid);

            if (measurementGids.Count == 0)
            {
                Logger.LogWarning($"{baseLogString} SendSingleScadaCommandAsync => Element with gid: 0x{breakerGid:X16} has no measurements.");
                return false;
            }

            var measurementGid = measurementGids.FirstOrDefault();
            if (measurementGid == 0)
            {
                Logger.LogWarning($"{baseLogString} SendSingleScadaCommandAsync => Measurement gid is 0.");
                return false;
            }

            var switchStatusCommandingClient = SwitchStatusCommandingClient.CreateClient();

            bool sendCommandSuccess = false;

            if(discreteCommandingType == DiscreteCommandingType.OPEN)
            {
                sendCommandSuccess = await switchStatusCommandingClient.SendOpenCommand(measurementGid);
            }
            else if(discreteCommandingType == DiscreteCommandingType.CLOSE)
            {
                sendCommandSuccess = await switchStatusCommandingClient.SendCloseCommand(measurementGid);
            }

            return sendCommandSuccess;
        }

        public async Task<bool> SendMultipleScadaCommandAsync(Dictionary<long, DiscreteCommandingType> elementGidCommandMap, Dictionary<long, CommandedElement> commandedElements, CommandOriginType commandOriginType)
        {
            var measurementMapClient = MeasurementMapClient.CreateClient();
            var elementToMeasurementMap = await measurementMapClient.GetElementToMeasurementMap();

            var commands = new Dictionary<long, int>();

            foreach (var elementGid in elementGidCommandMap.Keys)
            {
                var discreteCommandingType = elementGidCommandMap[elementGid];

                int reTryCount = 60;
                while (commandedElements.ContainsKey(elementGid) && commandedElements[elementGid].CommandingType == discreteCommandingType)
                {
                    Logger.LogDebug($"{baseLogString} SendMultipleScadaCommandAsync => Trying to send duplicate command. Entering delay for 1000 ms and retrying the call.");

                    await Task.Delay(1000);

                    if(--reTryCount <= 0)
                    {
                        Logger.LogError($"{baseLogString} SendMultipleScadaCommandAsync => Trying to send duplicate command. ReTryCount reached 60 calls.");
                        return false;
                    }
                }

                if(!elementToMeasurementMap.TryGetValue(elementGid, out List<long> measurementGids) || measurementGids.Count == 0)
                {
                    Logger.LogWarning($"{baseLogString} SendMultipleScadaCommandAsync => Element with gid: 0x{elementGid:X16} has no measurements.");
                    return false;
                }

                var measurementGid = measurementGids.FirstOrDefault();
                if (measurementGid == 0)
                {
                    Logger.LogWarning($"{baseLogString} SendMultipleScadaCommandAsync => Measurement gid is 0.");
                    return false;
                }

                commands.Add(measurementGid, (int)elementGidCommandMap[elementGid]);
            }

            var measurementProviderClient = MeasurementProviderClient.CreateClient();
            return await measurementProviderClient.SendMultipleDiscreteCommand(commands, commandOriginType);
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

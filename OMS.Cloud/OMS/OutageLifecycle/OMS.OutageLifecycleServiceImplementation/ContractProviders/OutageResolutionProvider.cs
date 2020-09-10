using Common.OMS;
using Common.OmsContracts.DataContracts.OutageDatabaseModel;
using Common.OmsContracts.OutageLifecycle;
using Common.PubSubContracts.DataContracts.CE;
using Microsoft.ServiceFabric.Data;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.WcfClient.OMS.ModelAccess;
using OMS.Common.WcfClient.OMS.ModelProvider;
using OMS.OutageLifecycleImplementation.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.OutageLifecycleImplementation.ContractProviders
{
    public class OutageResolutionProvider : IOutageResolutionContract
    {
        private readonly string baseLogString;
        private readonly OutageLifecycleHelper lifecycleHelper;
        private readonly OutageMessageMapper outageMessageMapper;

        private ICloudLogger logger;

        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        public OutageResolutionProvider(OutageLifecycleHelper lifecycleHelper)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            this.lifecycleHelper = lifecycleHelper;
            this.outageMessageMapper = new OutageMessageMapper();
        }

        #region IOutageResolutionContract
        public async Task<bool> ResolveOutage(long outageId)
        {
            Logger.LogVerbose($"{baseLogString} ResolveOutage method started. OutageId: {outageId}");

            try
            {
                var result = await lifecycleHelper.GetValidatedRepairedOutage(outageId);

                if (!result.HasValue)
                {
                    Logger.LogError($"{baseLogString} ResolveOutage => GetValidatedRepairedOutage did not return a value. OutageId: {outageId}");
                    return false;
                }

                var outageDbEntity = result.Value;
                outageDbEntity.ArchivedTime = DateTime.UtcNow;
                outageDbEntity.OutageState = OutageState.ARCHIVED;

                var outageModelAccessClient = OutageModelAccessClient.CreateClient();
                await outageModelAccessClient.UpdateOutage(outageDbEntity);
                Logger.LogInformation($"{baseLogString} ResolveOutage => Outage on element with gid: 0x{outageDbEntity.OutageElementGid:x16} is SUCCESSFULLY archived.");

                return await lifecycleHelper.PublishOutageAsync(Topic.ARCHIVED_OUTAGE, outageMessageMapper.MapOutageEntity(outageDbEntity));
            }
            catch (Exception e)
            {
                string message = $"{baseLogString} ResolveOutage => Exception: {e.Message}";
                Logger.LogError(message, e);

                return false;
            }
        }

        public async Task<bool> ValidateResolveConditions(long outageId)
        {
            Logger.LogVerbose($"{baseLogString} ValidateResolveConditions method started.  OutageId: {outageId}");

            try
            {
                var result = await lifecycleHelper.GetRepairedOutage(outageId);

                if (!result.HasValue)
                {
                    Logger.LogError($"{baseLogString} ValidateResolveConditions => GetRepairedOutage did not return a value.  OutageId: {outageId}");
                    return false;
                }

                var outageDbEntity = result.Value;

                var isolationPoints = new List<Equipment>();
                isolationPoints.AddRange(outageDbEntity.DefaultIsolationPoints);
                isolationPoints.AddRange(outageDbEntity.OptimumIsolationPoints);

                var outageModelReadAccessClient = OutageModelReadAccessClient.CreateClient();
                var topology = await outageModelReadAccessClient.GetTopologyModel();

                bool resolveCondition = true;

                foreach (Equipment isolationPoint in isolationPoints)
                {
                    if (!topology.GetElementByGid(isolationPoint.EquipmentId, out OutageTopologyElement element))
                    {
                        string errorMessage = $"{baseLogString} ValidateResolveConditions => element with gid 0x{isolationPoint.EquipmentId:X16} not found in current {ReliableDictionaryNames.OutageTopologyModel}";
                        Logger.LogError(errorMessage);
                        throw new Exception(errorMessage);
                        //MODO: soft handle
                        //resolveCondition = false;
                        //break;
                    }

                    if (element.NoReclosing != element.IsActive)
                    {
                        resolveCondition = false;
                        break;
                    }
                }

                outageDbEntity.IsResolveConditionValidated = resolveCondition;

                var outageModelAccessClient = OutageModelAccessClient.CreateClient();
                await outageModelAccessClient.UpdateOutage(outageDbEntity);
                
                return await lifecycleHelper.PublishOutageAsync(Topic.ACTIVE_OUTAGE, outageMessageMapper.MapOutageEntity(outageDbEntity));
            }
            catch (Exception e)
            {
                string message = $"{baseLogString} ValidateResolveConditions => Exception: {e.Message}";
                Logger.LogError(message, e);

                return false;
            }
        }

        public Task<bool> IsAlive()
        {
            return Task.Run(() => { return true; });
        }
        #endregion IOutageResolutionContract
    }
}

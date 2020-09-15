using Common.OMS;
using Common.OmsContracts.DataContracts.OutageDatabaseModel;
using Common.OmsContracts.OutageLifecycle;
using Common.PubSubContracts.DataContracts.CE;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
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
        private readonly IReliableStateManager stateManager;
        private readonly OutageLifecycleHelper lifecycleHelper;
        private readonly OutageMessageMapper outageMessageMapper;

        private ICloudLogger logger;

        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        #region Reliable Dictionaries
        private bool isOutageTopologyModelInitialized;

        private bool ReliableDictionariesInitialized
        {
            get
            {
                return isOutageTopologyModelInitialized;
            }
        }

        private ReliableDictionaryAccess<string, OutageTopologyModel> outageTopologyModel;
        private ReliableDictionaryAccess<string, OutageTopologyModel> OutageTopologyModel
        {
            get { return outageTopologyModel; }
        }

        private async void OnStateManagerChangedHandler(object sender, NotifyStateManagerChangedEventArgs e)
        {
            if (e.Action == NotifyStateManagerChangedAction.Add)
            {
                var operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
                string reliableStateName = operation.ReliableState.Name.AbsolutePath;

                if (reliableStateName == ReliableDictionaryNames.OutageTopologyModel)
                {
                    this.outageTopologyModel = await ReliableDictionaryAccess<string, OutageTopologyModel>.Create(stateManager, ReliableDictionaryNames.OutageTopologyModel);
                    this.isOutageTopologyModelInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.OutageTopologyModel}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
            }
        }
        #endregion Reliable Dictionaries

        public OutageResolutionProvider(IReliableStateManager stateManager, OutageLifecycleHelper lifecycleHelper)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            this.lifecycleHelper = lifecycleHelper;
            this.outageMessageMapper = new OutageMessageMapper();

            this.isOutageTopologyModelInitialized = false;

            this.stateManager = stateManager;
            this.stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;
        }

        #region IOutageResolutionContract
        public async Task<bool> ResolveOutage(long outageId)
        {
            Logger.LogVerbose($"{baseLogString} ResolveOutage method started. OutageId: {outageId}");

            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

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

            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

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

                var enumerableTopology = await OutageTopologyModel.GetEnumerableDictionaryAsync();

                if (!enumerableTopology.ContainsKey(ReliableDictionaryNames.OutageTopologyModel))
                {
                    Logger.LogError($"{baseLogString} Start => Topology not found in Rel Dictionary: {ReliableDictionaryNames.OutageTopologyModel}.");
                    return false;
                }

                var topology = enumerableTopology[ReliableDictionaryNames.OutageTopologyModel];

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

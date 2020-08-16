using Common.OmsContracts.ModelProvider;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.PubSubContracts.Interfaces;
using System;
using System.Threading.Tasks;
using ReliableDictionaryNames = Common.OMS.ReliableDictionaryNames;

namespace OMS.ModelProviderImplementation.ContractProviders
{
    public class OutageModelUpdateAccessProvider : IOutageModelUpdateAccessContract
    {
        private readonly string baseLogString;
        private readonly IReliableStateManager stateManager;

        #region Private Properties
        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }
        #endregion Private Properties

        #region Reliable Dictionaries
        private bool isCommandedElementsInitialized;
        private bool isOptimumIsolatioPointsInitialized;
        private bool isPotentialOutageInitialized;
        private bool ReliableDictionariesInitialized
        {
            get
            {
                return isCommandedElementsInitialized &&
                       isOptimumIsolatioPointsInitialized &&
                       isPotentialOutageInitialized;
            }
        }

        private ReliableDictionaryAccess<long, long> commandedElements;
        private ReliableDictionaryAccess<long, long> CommandedElements
        {
            get { return commandedElements; }
        }

        private ReliableDictionaryAccess<long, long> optimumIsloationPoints;
        private ReliableDictionaryAccess<long, long> OptimumIsolatioPoints
        {
            get { return optimumIsloationPoints; }
        }

        private ReliableDictionaryAccess<long, CommandOriginType> potentialOutage;
        private ReliableDictionaryAccess<long, CommandOriginType> PotentialOutage
        {
            get { return potentialOutage; }
        }

        private ReliableDictionaryAccess<long, IOutageTopologyModel> topologyModel;
        private ReliableDictionaryAccess<long, IOutageTopologyModel> TopologyModel
        {
            get { return topologyModel; }
        }

        private async void OnStateManagerChangedHandler(object sender, NotifyStateManagerChangedEventArgs e)
        {
            if (e.Action == NotifyStateManagerChangedAction.Add)
            {
                var operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
                string reliableStateName = operation.ReliableState.Name.AbsolutePath;
                
                if (reliableStateName == ReliableDictionaryNames.CommandedElements)
                {
                    commandedElements = await ReliableDictionaryAccess<long, long>.Create(this.stateManager, ReliableDictionaryNames.CommandedElements);
                    this.isCommandedElementsInitialized = true;
                }
                else if (reliableStateName == ReliableDictionaryNames.OptimumIsolatioPoints)
                {
                    optimumIsloationPoints = await ReliableDictionaryAccess<long, long>.Create(this.stateManager, ReliableDictionaryNames.OptimumIsolatioPoints);
                    this.isOptimumIsolatioPointsInitialized = true;
                }
                else if (reliableStateName == ReliableDictionaryNames.PotentialOutage)
                {
                    potentialOutage = await ReliableDictionaryAccess<long, CommandOriginType>.Create(this.stateManager, ReliableDictionaryNames.PotentialOutage);
                    this.isPotentialOutageInitialized = true;
                }
            }
        }
        #endregion Reliable Dictionaries

        public OutageModelUpdateAccessProvider(IReliableStateManager stateManager)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");

            isCommandedElementsInitialized = false;
            isOptimumIsolatioPointsInitialized = false;
            isPotentialOutageInitialized = false;

            this.stateManager = stateManager;
            this.stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;
        }

        #region IOutageModelUpdateAccessContract
        public async Task UpdateCommandedElements(long gid, ModelUpdateOperationType modelUpdateOperationType)
        {
            Logger.LogDebug("UpdateCommandedElements method started.");
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            try
            {
                if (modelUpdateOperationType == ModelUpdateOperationType.INSERT)
                {
                    if (!await CommandedElements.ContainsKeyAsync(gid))
                    {
                        await CommandedElements.SetAsync(gid, 0);
                    }
                }
                else if(modelUpdateOperationType == ModelUpdateOperationType.DELETE)
                {
                    await CommandedElements.TryRemoveAsync(gid);
                }
                else if(modelUpdateOperationType == ModelUpdateOperationType.CLEAR)
                {
                    await CommandedElements.ClearAsync();
                }
            }
            catch (Exception e)
            {
                string message = $"{baseLogString} UpdateCommandedElements => Exception: {e.Message}";
                Logger.LogError(message, e);
            }       
        }

        public async Task UpdateOptimumIsolationPoints(long gid, ModelUpdateOperationType modelUpdateOperationType)
        {
            Logger.LogDebug("UpdateOptimumIsolationPoints method started.");
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            try
            {
                if (modelUpdateOperationType == ModelUpdateOperationType.INSERT)
                {
                    if (!await OptimumIsolatioPoints.ContainsKeyAsync(gid))
                    {
                        await OptimumIsolatioPoints.SetAsync(gid, 0);
                    }
                }
                else if (modelUpdateOperationType == ModelUpdateOperationType.DELETE)
                {
                    await OptimumIsolatioPoints.TryRemoveAsync(gid);
                }
                else if (modelUpdateOperationType == ModelUpdateOperationType.CLEAR)
                {
                    await OptimumIsolatioPoints.ClearAsync();
                }
            }
            catch (Exception e)
            {
                string message = $"{baseLogString} UpdateOptimumIsolationPoints => Exception: {e.Message}";
                Logger.LogError(message, e);
            }
        }

        public async Task UpdatePotentialOutage(long gid , CommandOriginType commandOriginType, ModelUpdateOperationType modelUpdateOperationType)
        {
            Logger.LogDebug("UpdatePotentialOutage method started.");
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            try
            {
                if (modelUpdateOperationType == ModelUpdateOperationType.INSERT)
                {
                    if (!await PotentialOutage.ContainsKeyAsync(gid))
                    {
                        await PotentialOutage.SetAsync(gid, commandOriginType);
                    }
                }
                else if (modelUpdateOperationType == ModelUpdateOperationType.DELETE)
                {
                    await PotentialOutage.TryRemoveAsync(gid);
                }
                else if (modelUpdateOperationType == ModelUpdateOperationType.CLEAR)
                {
                    await PotentialOutage.ClearAsync();
                }
            }
            catch (Exception e)
            {
                string message = $"{baseLogString} UpdatePotentialOutage => Exception: {e.Message}";
                Logger.LogError(message, e);
            }
        }

        public async Task UpdateTopologyModel(IOutageTopologyModel outageTopologyModel)
		{
            Logger.LogDebug("UpdateTopologyModel method started.");
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            try
            {
                await TopologyModel.SetAsync(0, outageTopologyModel);
            }
            catch (Exception e)
            {
                string message = $"{baseLogString} UpdateTopologyModel => Exception: {e.Message}";
                Logger.LogError(message, e);
            }
        }
        
        public Task<bool> IsAlive()
        {
            return Task.Run(() => { return true; });
        }
        #endregion IOutageModelUpdateAccessContract
    }
}

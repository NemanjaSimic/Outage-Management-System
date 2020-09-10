using Common.OMS;
using Common.OmsContracts.ModelProvider;
using Common.PubSubContracts.DataContracts.CE;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.PubSubContracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.ModelProviderImplementation.ContractProviders
{
    public class OutageModelReadAccessProvider : IOutageModelReadAccessContract
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
        private bool isTopologyModelInitialized;
        private bool isCommandedElementsInitialized;
        private bool isOptimumIsolatioPointsInitialized;
        private bool isPotentialOutageInitialized;
        public bool ReliableDictionariesInitialized
        {
            get
            {
                return isTopologyModelInitialized &&
                       isCommandedElementsInitialized &&
                       isOptimumIsolatioPointsInitialized &&
                       isPotentialOutageInitialized;
            }
        }

        private ReliableDictionaryAccess<long, OutageTopologyModel> topologyModel;
        private ReliableDictionaryAccess<long, OutageTopologyModel> TopologyModel
        {
            get { return topologyModel; }
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

        private async void OnStateManagerChangedHandler(object sender, NotifyStateManagerChangedEventArgs e)
        {
            if (e.Action == NotifyStateManagerChangedAction.Add)
            {
                var operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
                string reliableStateName = operation.ReliableState.Name.AbsolutePath;
                
                if (reliableStateName == ReliableDictionaryNames.OutageTopologyModel)
                {
                    topologyModel = await ReliableDictionaryAccess<long, OutageTopologyModel>.Create(this.stateManager, ReliableDictionaryNames.OutageTopologyModel);
                    this.isTopologyModelInitialized = true;
                }
                else if (reliableStateName == ReliableDictionaryNames.CommandedElements)
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

        public OutageModelReadAccessProvider(IReliableStateManager stateManager)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");

            isTopologyModelInitialized = false;
            isCommandedElementsInitialized = false;
            isOptimumIsolatioPointsInitialized = false;
            isPotentialOutageInitialized = false;

            this.stateManager = stateManager;
            this.stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;
        }

        #region IOutageModelReadAccessContract
        public async Task<Dictionary<long, long>> GetCommandedElements()
        {
            Logger.LogDebug("GetCommandedElements method started.");
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            var commandedElements = new Dictionary<long, long>();

            try
            {
                commandedElements = await CommandedElements.GetDataCopyAsync();
            }
            catch (Exception e)
            {
                string message = $"{baseLogString} GetCommandedElements => Exception: {e.Message}";
                Logger.LogError(message, e);
            }

            return commandedElements;
        }

        public async Task<Dictionary<long, long>> GetOptimumIsolatioPoints()
        {
            Logger.LogDebug("GetOptimumIsolatioPoints method started.");
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            var optimumIsolatioPoints = new Dictionary<long, long>();

            try
            {
                optimumIsolatioPoints = await OptimumIsolatioPoints.GetDataCopyAsync();
            }
            catch (Exception e)
            {
                string message = $"{baseLogString} GetOptimumIsolatioPoints => Exception: {e.Message}";
                Logger.LogError(message, e);
            }

            return optimumIsolatioPoints;
        }

        public async Task<Dictionary<long, CommandOriginType>> GetPotentialOutage()
        {
            Logger.LogDebug("GetPotentialOutage method started.");
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            var potentialOutage = new Dictionary<long, CommandOriginType>();

            try
            {
                potentialOutage = await PotentialOutage.GetDataCopyAsync();
            }
            catch (Exception e)
            {
                string message = $"{baseLogString} GetPotentialOutage => Exception: {e.Message}";
                Logger.LogError(message, e);
            }

            return potentialOutage;
        }

        public async Task<OutageTopologyModel> GetTopologyModel()
        {
            Logger.LogDebug("GetTopologyModel method started.");
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            //MODO: conditionalValue...
            OutageTopologyModel topologyModel = null;

            try
            {
                var result = await TopologyModel.GetDataCopyAsync();

                if(result.ContainsKey(0))
                {
                    topologyModel = result[0];
                }
            }
            catch (Exception e)
            {
                string message = $"{baseLogString} GetTopologyModel => Exception: {e.Message}";
                Logger.LogError(message, e);
            }

            return topologyModel;
        }

        public async Task<OutageTopologyElement> GetElementById(long gid)
        {
            Logger.LogDebug("GetElementById method started.");
            while (!ReliableDictionariesInitialized)
			{
                await Task.Delay(1000);
			}

            //MODO: conditionalValue...
            OutageTopologyElement topologyElement = null;

            try
            {
                var outageTopologyModelDictionary = await TopologyModel.GetDataCopyAsync();
                var outageTopologyModel = (await TopologyModel.GetDataCopyAsync())[0];
                outageTopologyModel.GetElementByGid(gid, out topologyElement);
            }
            catch (Exception e)
            {
                string message = $"{baseLogString} GetElementById => Exception: {e.Message}";
                Logger.LogError(message, e);
            }

            return topologyElement;
        }

        public Task<bool> IsAlive()
        {
            return Task.Run(() => { return true; });
        }
        #endregion
    }
}

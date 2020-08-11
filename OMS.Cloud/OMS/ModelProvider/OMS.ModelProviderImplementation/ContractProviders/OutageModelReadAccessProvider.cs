using Common.CE;
using Common.OMS;
using Common.OmsContracts.ModelProvider;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReliableDictionaryNames = Common.OMS.ReliableDictionaryNames;

namespace OMS.ModelProviderImplementation.ContractProviders
{
    public class OutageModelReadAccessProvider : IOutageModelReadAccessContract
    {
        private readonly IReliableStateManager stateManager;
        private bool isTopologyModelInitialized = false;
        private bool isCommandedElementsInitialized = false;
        private bool isOptimumIsolatioPointsInitialized = false;
        private bool isPotentialOutageInitialized = false;

        private ReliableDictionaryAccess<long, IOutageTopologyModel> topologyModel;

        public ReliableDictionaryAccess<long, IOutageTopologyModel> TopologyModel
        {
            get
            {
                    
                return topologyModel ?? (topologyModel = ReliableDictionaryAccess<long, IOutageTopologyModel>.Create(stateManager, ReliableDictionaryNames.OutageTopologyModel).Result);
                
            }

        }
        private ReliableDictionaryAccess<long, long> commandedElements;

        public ReliableDictionaryAccess<long, long> CommandedElements
        {
            get { return commandedElements ?? (commandedElements = ReliableDictionaryAccess<long, long>.Create(this.stateManager, ReliableDictionaryNames.CommandedElements).Result); }
        }

        private ReliableDictionaryAccess<long, long> optimumIsloationPoints;

        public ReliableDictionaryAccess<long, long> OptimumIsolatioPoints
        {
            get { return optimumIsloationPoints ?? (optimumIsloationPoints = ReliableDictionaryAccess<long, long>.Create(this.stateManager, ReliableDictionaryNames.OptimumIsolatioPoints).Result); }

        }

        private ReliableDictionaryAccess<long, CommandOriginType> potentialOutage;

        public ReliableDictionaryAccess<long, CommandOriginType> PotentialOutage
        {
            get { return potentialOutage ?? (potentialOutage = ReliableDictionaryAccess<long, CommandOriginType>.Create(this.stateManager, ReliableDictionaryNames.PotentialOutage).Result); }
        }
        public OutageModelReadAccessProvider(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
            this.stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;
        }

        public Task<bool> IsAlive()
        {
            return Task.Run(() => { return true; });
        }
        private async void OnStateManagerChangedHandler(object sender, NotifyStateManagerChangedEventArgs e)
        {
            if (e.Action == NotifyStateManagerChangedAction.Add)
            {
                var operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
                string reliableStateName = operation.ReliableState.Name.AbsolutePath;
                if (reliableStateName == ReliableDictionaryNames.OutageTopologyModel)
                {
                    topologyModel = await ReliableDictionaryAccess<long, IOutageTopologyModel>.Create(this.stateManager, ReliableDictionaryNames.OutageTopologyModel);
                    isTopologyModelInitialized = true;
                }
                else if (reliableStateName == ReliableDictionaryNames.CommandedElements)
                {
                    commandedElements = await ReliableDictionaryAccess<long, long>.Create(this.stateManager, ReliableDictionaryNames.CommandedElements);
                    isCommandedElementsInitialized = true;
                }
                else if (reliableStateName == ReliableDictionaryNames.OptimumIsolatioPoints)
                {
                    optimumIsloationPoints = await ReliableDictionaryAccess<long, long>.Create(this.stateManager, ReliableDictionaryNames.OptimumIsolatioPoints);
                    isOptimumIsolatioPointsInitialized = true;
                }
                else if (reliableStateName == ReliableDictionaryNames.PotentialOutage)
                {
                    potentialOutage = await ReliableDictionaryAccess<long, CommandOriginType>.Create(this.stateManager, ReliableDictionaryNames.PotentialOutage);
                    isPotentialOutageInitialized = true;
                }

            }
        }

        public bool ReliableDictionariesInitialized { get { return isTopologyModelInitialized && isCommandedElementsInitialized && isOptimumIsolatioPointsInitialized && isPotentialOutageInitialized; } }

        #region IOutageModelReadAccessContract Implementation
        public async Task<Dictionary<long, long>> GetCommandedElements()
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }
            return await CommandedElements.GetDataCopyAsync();
        }

        public async Task<Dictionary<long, long>> GetOptimumIsolatioPoints()
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }
            return await OptimumIsolatioPoints.GetDataCopyAsync();
        }

        public async Task<Dictionary<long, CommandOriginType>> GetPotentialOutage()
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }
            return await PotentialOutage.GetDataCopyAsync();
        }

        public async Task<IOutageTopologyModel> GetTopologyModel()
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }
            //Get topologyModel from CE service
            return (await TopologyModel.GetDataCopyAsync())[0];
        }

        public async Task<IOutageTopologyElement> GetElementById(long gid)
        {
            while (!ReliableDictionariesInitialized)
			{
                await Task.Delay(1000);
			}

			(await TopologyModel.GetDataCopyAsync())[0].GetElementByGid(gid, out IOutageTopologyElement outageTopologyElement);

            return outageTopologyElement;
        }
        #endregion

    }
}

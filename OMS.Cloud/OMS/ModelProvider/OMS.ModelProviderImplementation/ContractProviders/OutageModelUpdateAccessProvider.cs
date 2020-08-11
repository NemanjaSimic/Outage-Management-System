using Common.OMS;
using Common.OmsContracts.ModelProvider;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.ModelProviderImplementation.ContractProviders
{
    public class OutageModelUpdateAccessProvider : IOutageModelUpdateAccessContract
    {
        private readonly IReliableStateManager stateManager;
        private bool isCommandedElementsInitialized = false;
        private bool isOptimumIsolatioPointsInitialized = false;
        private bool isPotentialOutageInitialized = false;

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
        public OutageModelUpdateAccessProvider(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
            this.stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;
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

        public bool ReliableDictionariesInitialized { get { return  isCommandedElementsInitialized && isOptimumIsolatioPointsInitialized && isPotentialOutageInitialized; } }

        public async Task UpdateCommandedElements(long gid, ModelUpdateOperationType modelUpdateOperationType)
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }
            
            switch (modelUpdateOperationType)
            {
                case ModelUpdateOperationType.INSERT:
                    if(!(await CommandedElements.ContainsKeyAsync(gid)))
                    await CommandedElements.SetAsync(gid, 0);
                    break;
                case ModelUpdateOperationType.DELETE:
                    await CommandedElements.TryRemoveAsync(gid);
                    break;
                case ModelUpdateOperationType.CLEAR:
                    await CommandedElements.ClearAsync();
                    break;
                default:
                    break;
            }
                
        }

        public async Task UpdateOptimumIsolationPoints(long gid, ModelUpdateOperationType modelUpdateOperationType)
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }
            
            switch (modelUpdateOperationType)
            {
                case ModelUpdateOperationType.INSERT:
                    if (!(await OptimumIsolatioPoints.ContainsKeyAsync(gid)))
                        await OptimumIsolatioPoints.SetAsync(gid, 0);
                    break;
                case ModelUpdateOperationType.DELETE:
                    await OptimumIsolatioPoints.TryRemoveAsync(gid);
                    break;
                case ModelUpdateOperationType.CLEAR:
                    await OptimumIsolatioPoints.ClearAsync();
                    break;
                default:
                    break;
            }
            
        }

        public async Task UpdatePotentialOutage(long gid , CommandOriginType commandOriginType, ModelUpdateOperationType modelUpdateOperationType)
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }
            
            switch (modelUpdateOperationType)
            {
                case ModelUpdateOperationType.INSERT:
                    if (!(await PotentialOutage.ContainsKeyAsync(gid)))
                        await PotentialOutage.SetAsync(gid, commandOriginType);
                    break;
                case ModelUpdateOperationType.DELETE:
                    await PotentialOutage.TryRemoveAsync(gid);
                    break;
                case ModelUpdateOperationType.CLEAR:
                    await PotentialOutage.ClearAsync();
                    break;
                default:
                    break;
            }
        }

        public Task<bool> IsAlive()
        {
            return Task.Run(() => { return true; });
        }
    }
}

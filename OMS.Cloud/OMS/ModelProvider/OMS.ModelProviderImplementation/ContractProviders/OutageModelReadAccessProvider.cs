using Common.OMS;
using Common.OmsContracts.ModelProvider;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.ModelProviderImplementation.ContractProviders
{
    public class OutageModelReadAccessProvider : IOutageModelReadAccessContract
    {
        private readonly IReliableStateManager stateManager;
        private ReliableDictionaryAccess<long, IOutageTopologyModel> topologyModel;

        public ReliableDictionaryAccess<long, IOutageTopologyModel> TopologyModel
        {
            get
            {
                    
                return topologyModel ?? (topologyModel = ReliableDictionaryAccess<long, IOutageTopologyModel>.Create(stateManager, ReliableDictionaryNames.OutageTopologyModel).Result);
                
            }

        }
        public OutageModelReadAccessProvider(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
            this.stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;
        }
        public async Task<IOutageTopologyModel> GetTopologyModel()
        {
            //Get topologyModel from CE service
            return topologyModel[0];
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
                }
            }
        }
    }
}

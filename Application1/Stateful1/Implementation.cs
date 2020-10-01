using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stateful1
{
    class Implementation
    {
        private readonly string baseLogString;
        private readonly IReliableStateManager stateManager;

        #region Reliable Dictionaries
        private bool isInitialized;

        private bool ReliableDictionariesInitialized
        {
            get
            {
                return isInitialized;
            }
        }

        private ReliableDictionaryAccess<long, string> infoCache;
        private ReliableDictionaryAccess<long, string> InfoCache
        {
            get { return infoCache; }
        }

        private async void OnStateManagerChangedHandler(object sender, NotifyStateManagerChangedEventArgs eventArgs)
        {
            try
            {
                await InitializeReliableCollections(eventArgs);
            }
            catch (FabricNotPrimaryException)
            {
                
            }
        }

        private async Task InitializeReliableCollections(NotifyStateManagerChangedEventArgs e)
        {
            if (e.Action == NotifyStateManagerChangedAction.Add)
            {
                var operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
                string reliableStateName = operation.ReliableState.Name.AbsolutePath;

                if (reliableStateName == "InfoCache")
                {
                    infoCache = await ReliableDictionaryAccess<long, string>.Create(stateManager, "InfoCache");
                    isInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => 'InfoCache' ReliableDictionaryAccess initialized.";
                }
            }
            else if (e.Action == NotifyStateManagerChangedAction.Rebuild)
            {
                var operation = e as NotifyStateManagerRebuildEventArgs;
                var enumerator = operation.ReliableStates.GetAsyncEnumerator();
              
                while(await enumerator.MoveNextAsync(new System.Threading.CancellationToken()))
                {
                    var reliableState = enumerator.Current;
                    var name = reliableState.Name;
                }
            }
            else if (e.Action == NotifyStateManagerChangedAction.Remove)
            {

            }
        }
        #endregion Reliable Dictionaries

        public Implementation(IReliableStateManager stateManager)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            this.isInitialized = false;

            this.stateManager = stateManager;
            this.stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;
        }

        public async Task Start()
        {
            while(!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            var enumerableInfoCache = InfoCache.GetEnumerableDictionaryAsync();
            await InfoCache.SetAsync(0, "Hello World!");
        }
    }
}

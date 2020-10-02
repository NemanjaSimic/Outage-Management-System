using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Names;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.NmsContracts.GDA;
using OMS.Common.SCADA;
using OMS.Common.TmsContracts.Notifications;
using OMS.Common.WcfClient.TMS;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SCADA.ModelProviderImplementation.DistributedTransaction
{
    public class ScadaNotifyNetworkModelUpdate : INotifyNetworkModelUpdateContract
    {
        private readonly string baseLogString;
        private readonly IReliableStateManager stateManager;

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        #region Reliable Dictionaries
        private bool isModelChangesInitialized;
        private bool ReliableDictionariesInitialized
        {
            get { return isModelChangesInitialized; }
        }

        private ReliableDictionaryAccess<byte, List<long>> ModelChanges { get; set; }

        private async void OnStateManagerChangedHandler(object sender, NotifyStateManagerChangedEventArgs eventArgs)
        {
            try
            {
                await InitializeReliableCollections(eventArgs);
            }
            catch (FabricNotPrimaryException)
            {
                Logger.LogDebug($"{baseLogString} OnStateManagerChangedHandler => NotPrimaryException. To be ignored.");
            }
            catch (FabricObjectClosedException)
            {
                Logger.LogDebug($"{baseLogString} OnStateManagerChangedHandler => FabricObjectClosedException. To be ignored.");
            }
            catch (COMException)
            {
                Logger.LogDebug($"{baseLogString} OnStateManagerChangedHandler => {typeof(COMException)}. To be ignored.");
            }
        }

        private async Task InitializeReliableCollections(NotifyStateManagerChangedEventArgs e)
        {
            if (e.Action == NotifyStateManagerChangedAction.Add)
            {
                var operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
                string reliableStateName = operation.ReliableState.Name.AbsolutePath;

                if (reliableStateName == ReliableDictionaryNames.ModelChanges)
                {
                    ModelChanges = await ReliableDictionaryAccess<byte, List<long>>.Create(stateManager, ReliableDictionaryNames.ModelChanges);
                    this.isModelChangesInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.ModelChanges}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
            }
        }
        #endregion Reliable Dictionaries

        public ScadaNotifyNetworkModelUpdate(IReliableStateManager stateManager)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            this.isModelChangesInitialized = false;

            this.stateManager = stateManager;
            this.stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;
        }

        #region IModelUpdateNotificationContract
        public async Task<bool> Notify(Dictionary<DeltaOpType, List<long>> modelChanges)
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            StashChanges(modelChanges);

            var transactionEnlistmentClient = TransactionEnlistmentClient.CreateClient();
            bool success = await transactionEnlistmentClient.Enlist(DistributedTransactionNames.NetworkModelUpdateTransaction, MicroserviceNames.ScadaModelProviderService);

            if (success)
            {
                Logger.LogInformation($"{baseLogString} Notify => SCADA SUCCESSFULLY notified about network model update.");
            }
            else
            {
                Logger.LogInformation($"{baseLogString} Notify => SCADA UNSUCCESSFULLY notified about network model update.");
            }

            return success;
        }

        public Task<bool> IsAlive()
        {
            return Task.Run(() => { return true; });
        }
        #endregion IModelUpdateNotificationContract
    
        private void StashChanges(Dictionary<DeltaOpType, List<long>> modelChanges)
        {
            var tasks = new List<Task>();

            foreach (var element in modelChanges)
            {
                tasks.Add(ModelChanges.SetAsync((byte)element.Key, element.Value));
            }

            Task.WaitAll(tasks.ToArray());
        }
    }
}

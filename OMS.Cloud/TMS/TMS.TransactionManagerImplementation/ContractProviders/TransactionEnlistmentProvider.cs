using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.TmsContracts;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading.Tasks;

namespace TMS.TransactionManagerImplementation.ContractProviders
{
    public class TransactionEnlistmentProvider : ITransactionEnlistmentContract
    {
        private readonly string baseLogString;
        private readonly IReliableStateManager stateManager;

        #region Reliable Dictionaries
        private bool isActiveTransactionsInitialized;
        private bool isTransactionEnlistmentLedgerInitialized;

        private bool ReliableDictionariesInitialized
        {
            get 
            { 
                return isActiveTransactionsInitialized &&
                       isTransactionEnlistmentLedgerInitialized;
            }
        }

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        private ReliableDictionaryAccess<string, HashSet<string>> activeTransactions;
        /// <summary>
        /// key - transaction name from OMS.Common.Cloud.Names.DistributedTransactionNames (e.g. 'NetworkModelUpdateTransaction'),
        /// value - HashSet of services that are part of the transaction, represented by their URI (e.g. 'fabric:/OMS.Cloud/SCADA.ModelProviderService')
        /// </summary>
        private ReliableDictionaryAccess<string, HashSet<string>> ActiveTransactions
        {
            get { return activeTransactions; }
        }

        private ReliableDictionaryAccess<string, HashSet<string>> transactionEnlistmentLedger;
        /// <summary>
        /// key - transaction name from OMS.Common.Cloud.Names.DistributedTransactionNames (e.g. 'NetworkModelUpdateTransaction'),
        /// value - Dictionary of enlisted transaction actors =>
        ///         key - service URI (e.g. 'fabric:/OMS.Cloud/SCADA.ModelProviderService'),
        ///         value - TransactionActor object - uri and service type
        /// </summary>
        private ReliableDictionaryAccess<string, HashSet<string>> TransactionEnlistmentLedger
        {
            get { return transactionEnlistmentLedger; }
        }

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
        }

        private async Task InitializeReliableCollections(NotifyStateManagerChangedEventArgs e)
        {
            if (e.Action == NotifyStateManagerChangedAction.Add)
            {
                var operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
                string reliableStateName = operation.ReliableState.Name.AbsolutePath;

                if (reliableStateName == ReliableDictionaryNames.ActiveTransactions)
                {
                    activeTransactions = await ReliableDictionaryAccess<string, HashSet<string>>.Create(stateManager, ReliableDictionaryNames.ActiveTransactions);
                    isActiveTransactionsInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.ActiveTransactions}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.TransactionEnlistmentLedger)
                {
                    transactionEnlistmentLedger = await ReliableDictionaryAccess<string, HashSet<string>>.Create(stateManager, ReliableDictionaryNames.TransactionEnlistmentLedger);
                    isTransactionEnlistmentLedgerInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.TransactionEnlistmentLedger}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
            }
        }
        #endregion Reliable Dictionaries

        public TransactionEnlistmentProvider(IReliableStateManager stateManager)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            this.isActiveTransactionsInitialized = false;
            this.isTransactionEnlistmentLedgerInitialized = false;

            this.stateManager = stateManager;
            stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;
        }

        #region ITransactionEnlistmentContract
        public async Task<bool> Enlist(string transactionName, string transactionActorName)
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            var enumerableActiveTransactions = await ActiveTransactions.GetEnumerableDictionaryAsync();
            if (!enumerableActiveTransactions.ContainsKey(transactionName))
            {
                string errorMessage = $"{baseLogString} Enlist => transaction '{transactionName}' not found in '{ReliableDictionaryNames.ActiveTransactions}'.";
                Logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            var result = await TransactionEnlistmentLedger.TryGetValueAsync(transactionName);
            if (!result.HasValue)
            {
                string errorMessage = $"{baseLogString} Enlist => Transaction '{transactionName}' not found in '{ReliableDictionaryNames.TransactionEnlistmentLedger}'.";
                Logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            var transactionLedger = result.Value;

            if(!enumerableActiveTransactions[transactionName].Contains(transactionActorName))
            {
                string errorMessage = $"{baseLogString} InvokePreparationOnActors => Transaction '{transactionName}' not found in '{ReliableDictionaryNames.TransactionEnlistmentLedger}'.";
                Logger.LogError(errorMessage);
                return false;
            }

            transactionLedger.Add(transactionActorName);
            await TransactionEnlistmentLedger.SetAsync(transactionName, transactionLedger);
            return true;
        }

        public Task<bool> IsAlive()
        {
            return Task.Run(() => { return true; });
        }
        #endregion ITransactionCoordinatorContract
    }
}

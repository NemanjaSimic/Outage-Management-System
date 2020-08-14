using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.TmsContracts;
using OMS.Common.WcfClient.TMS;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TMS.TransactionManagerImplementation.ContractProviders
{
    public class TransactionCoordinatorProvider : ITransactionCoordinatorContract
    {
        private readonly string baseLogString;
        private readonly IReliableStateManager stateManager;

        #region Private Properties
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
        #endregion Private Properties

        public TransactionCoordinatorProvider(IReliableStateManager stateManager)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            this.isActiveTransactionsInitialized = false;
            this.isTransactionEnlistmentLedgerInitialized = false;

            this.stateManager = stateManager;
            stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;
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

        #region ITransactionCoordinatorContract
        public async Task StartDistributedTransaction(string transactionName, IEnumerable<string> transactionActors)
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            if(await ActiveTransactions.ContainsKeyAsync(transactionName))
            {
                string warnMessage = $"{baseLogString} StartDistributedUpdate => Transaction with name '{transactionName}' has already been stated.";
                Logger.LogWarning(warnMessage);
                return;
            }

            var actorsHashSet = new HashSet<string>(transactionActors);
            await ActiveTransactions.SetAsync(transactionName, actorsHashSet);
            await TransactionEnlistmentLedger.SetAsync(transactionName, new HashSet<string>());

            var actorsSb = new StringBuilder();
            foreach (var actor in actorsHashSet)
            {
                actorsSb.Append($"'{actor}', ");
            }

            Logger.LogInformation($"{baseLogString} StartDistributedUpdate => Distributed transaction '{transactionName}' SUCCESSFULLY started. Waiting for transaction actors [{actorsSb}] to enlist.");
        }

        public async Task FinishDistributedTransaction(string transactionName, bool success)
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            if (success)
            {
                bool prepareSuccess = false;
                int retryCount = 30;

                while(--retryCount > 0)
                {
                    try
                    {
                        prepareSuccess = await InvokePreparationOnActors(transactionName);
                        break;
                    }
                    catch(NotAllActorsEnlistedException)
                    {
                        await Task.Delay(2000);
                        continue;
                    }
                    catch (Exception e)
                    {
                        string errorMessage = $"{baseLogString} FinishDistributedTransaction => Exception in InvokePreparationOnActors: {e.Message}";
                        Logger.LogError(errorMessage);
                        break;
                    }
                }

                if(prepareSuccess)
                {
                    await InvokeCommitOnActors(transactionName);
                }
                else
                {
                    await InvokeRollbackOnActors(transactionName);
                }

                Logger.LogInformation($"{baseLogString} FinishDistributedUpdate => Distributed transaction finsihed SUCCESSFULLY.");
            }
            else
            {
                Logger.LogInformation($"{baseLogString} FinishDistributedUpdate => Distributed transaction finsihed UNSUCCESSFULLY.");
            }

            await ActiveTransactions.TryRemoveAsync(transactionName);
            await TransactionEnlistmentLedger.TryRemoveAsync(transactionName);
        }
        #endregion ITransactionCoordinatorContract

        #region Private Members
        private async Task<bool> InvokePreparationOnActors(string transactionName)
        {
            bool success;

            var enumerableActiveTransactions = await ActiveTransactions.GetEnumerableDictionaryAsync();
            if (!enumerableActiveTransactions.ContainsKey(transactionName))
            {
                string errorMessage = $"{baseLogString} InvokePreparationOnActors => transaction '{transactionName}' not found in '{ReliableDictionaryNames.ActiveTransactions}'.";
                Logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            var result = await TransactionEnlistmentLedger.TryGetValueAsync(transactionName);
            if (!result.HasValue)
            {
                string errorMessage = $"{baseLogString} InvokePreparationOnActors => Transaction '{transactionName}' not found in '{ReliableDictionaryNames.TransactionEnlistmentLedger}'.";
                Logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            var transactionLedger = result.Value;

            if(!enumerableActiveTransactions[transactionName].SetEquals(transactionLedger))
            {
                string errorMessage = $"{baseLogString} InvokePreparationOnActors => not all actors have enlisted for the transaction '{transactionName}'.";
                Logger.LogError(errorMessage);
                throw new NotAllActorsEnlistedException(errorMessage);
            }

            List<Task<Tuple<string, bool>>> tasks = new List<Task<Tuple<string, bool>>>();

            foreach (var transactionActorName in transactionLedger)
            {
                var task = Task.Run(async () =>
                {
                    ITransactionActorContract transactionActorClient = TransactionActorClient.CreateClient(transactionActorName);
                    var prepareSuccess = await transactionActorClient.Prepare();
                    Logger.LogInformation($"{baseLogString} InvokePreparationOnActors => Prepare invoked on Transaction actor: {transactionActorName}, Success: {prepareSuccess}.");
                    
                    return new Tuple<string, bool>(transactionActorName, prepareSuccess);
                });

                tasks.Add(task);
            }

            var taskResults = await Task.WhenAll(tasks);
            success = true;

            foreach (var taskResult in taskResults)
            {
                var actorUri = taskResult.Item1;
                var prepareSuccess = taskResult.Item2;

                success = success && prepareSuccess;

                if (success)
                {
                    Logger.LogInformation($"{baseLogString} InvokePreparationOnActors => Preparation on Transaction actor: {actorUri} finsihed SUCCESSFULLY.");
                }
                else
                {
                    Logger.LogInformation($"{baseLogString} InvokePreparationOnActors => Preparation on Transaction actor: {actorUri} finsihed UNSUCCESSFULLY.");
                    break;
                }
            }

            return success;
        }

        private async Task InvokeCommitOnActors(string transactionName)
        {
            var enumerableActiveTransactions = await ActiveTransactions.GetEnumerableDictionaryAsync();
            if(!enumerableActiveTransactions.ContainsKey(transactionName))
            {
                string errorMessage = $"{baseLogString} InvokeCommitOnActors => transaction '{transactionName}' not found in '{ReliableDictionaryNames.ActiveTransactions}'.";
                Logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            var result = await TransactionEnlistmentLedger.TryGetValueAsync(transactionName);
            if(!result.HasValue)
            {
                string errorMessage = $"{baseLogString} InvokeCommitOnActors => Transaction '{transactionName}' not found in '{ReliableDictionaryNames.TransactionEnlistmentLedger}'.";
                Logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            var transactionLedger = result.Value;

            List<Task> tasks = new List<Task>();

            foreach (var transactionActorName in transactionLedger)
            {
                if(enumerableActiveTransactions[transactionName].Contains(transactionActorName))
                {
                    var task = Task.Run(async () =>
                    {
                        ITransactionActorContract transactionActorClient = TransactionActorClient.CreateClient(transactionActorName);
                        await transactionActorClient.Commit();
                        Logger.LogInformation($"{baseLogString} InvokeCommitOnActors => Commit invoked on Transaction actor: {transactionActorName}.");
                    });

                    tasks.Add(task);                    
                }
            }

            Task.WaitAll(tasks.ToArray());
            Logger.LogInformation($"{baseLogString} InvokeCommitOnActors => Commit SUCCESSFULLY invoked on all transaction actors.");
        }

        private async Task InvokeRollbackOnActors(string transactionName)
        {
            var enumerableActiveTransactions = await ActiveTransactions.GetEnumerableDictionaryAsync();
            if (!enumerableActiveTransactions.ContainsKey(transactionName))
            {
                string errorMessage = $"{baseLogString} InvokeRollbackOnActors => transaction '{transactionName}' not found in '{ReliableDictionaryNames.ActiveTransactions}'.";
                Logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            var result = await TransactionEnlistmentLedger.TryGetValueAsync(transactionName);
            if (!result.HasValue)
            {
                string errorMessage = $"{baseLogString} InvokeRollbackOnActors => Transaction '{transactionName}' not found in '{ReliableDictionaryNames.TransactionEnlistmentLedger}'.";
                Logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            var transactionLedger = result.Value;

            List<Task> tasks = new List<Task>();

            foreach (var transactionActorName in transactionLedger)
            {
                if (enumerableActiveTransactions[transactionName].Contains(transactionActorName))
                {
                    var task = Task.Run(async () =>
                    {
                        ITransactionActorContract transactionActorClient = TransactionActorClient.CreateClient(transactionActorName);
                        await transactionActorClient.Rollback();
                        Logger.LogInformation($"{baseLogString} InvokeRollbackOnActors => Rollback invoked on Transaction actor: {transactionActorName}.");
                    });

                    tasks.Add(task);
                }
            }

            Task.WaitAll(tasks.ToArray());
            Logger.LogInformation($"{baseLogString} InvokeRollbackOnActors => Rollback SUCCESSFULLY invoked on all transaction actors.");
        }
        #endregion Private Members
    }
}

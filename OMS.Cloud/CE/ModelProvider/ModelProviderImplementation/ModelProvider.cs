using Common.CE;
using Common.CeContracts;
using Common.CeContracts.ModelProvider;
using Common.CeContracts.TopologyProvider;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.TmsContracts;
using OMS.Common.WcfClient.CE;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CE.ModelProviderImplementation
{
	public class ModelProvider : IModelProviderContract
    {
        #region Fields
        private TransactionFlag transactionFlag;

        private ReliableDictionaryAccess<short, List<long>> energySourceCache;
        public ReliableDictionaryAccess<short, List<long>> EnergySourceCache { get => energySourceCache; }

        private ReliableDictionaryAccess<short, Dictionary<long, TopologyElement>> elementCache;
        public ReliableDictionaryAccess<short, Dictionary<long, TopologyElement>> ElementCache { get => elementCache; }

        private ReliableDictionaryAccess<short, Dictionary<long, List<long>>> elementConnectionCache;
        public ReliableDictionaryAccess<short, Dictionary<long, List<long>>> ElementConnectionCache { get => elementConnectionCache; }

        private ReliableDictionaryAccess<short, HashSet<long>> recloserCache;
        public ReliableDictionaryAccess<short, HashSet<long>> RecloserCache { get => recloserCache; }

        #endregion

        private ModelManager modelManager;

        private bool isElementCacheInitialized = false;
        private bool isElementConnectionCacheInitialized = false;
        private bool isEnergySourceCacheInitialized = false;
        private bool isRecloserCacheInitialized = false;

        private readonly ITopologyProviderContract topologyProviderClient;
        private readonly IMeasurementProviderContract measurementProviderClient;


        private readonly string baseLogString;
        private readonly IReliableStateManager stateManager;

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        private bool AreReliableDictionariesInitialized
        {
            get => isElementCacheInitialized 
                && isElementConnectionCacheInitialized 
                && isRecloserCacheInitialized 
                && isEnergySourceCacheInitialized;
        }

        public ModelProvider(IReliableStateManager stateManager)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            string verboseMessage = $"{baseLogString} entering Ctor.";
            Logger.LogVerbose(verboseMessage);

            this.topologyProviderClient = TopologyProviderClient.CreateClient();
            this.measurementProviderClient = MeasurementProviderClient.CreateClient();

            this.stateManager = stateManager;
            stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;

            transactionFlag = TransactionFlag.NoTransaction;
            modelManager = new ModelManager();

            this.isElementCacheInitialized = false;
            this.isElementConnectionCacheInitialized = false;
            this.isEnergySourceCacheInitialized = false;
            this.isRecloserCacheInitialized = false;

            string debugMessage = $"{baseLogString} Ctor => Clients initialized.";
            Logger.LogDebug(debugMessage);
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

                if (reliableStateName == ReliableDictionaryNames.ElementCache)
                {
                    elementCache = await ReliableDictionaryAccess<short, Dictionary<long, TopologyElement>>.Create(stateManager, ReliableDictionaryNames.ElementCache);
                    this.isElementCacheInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.ElementCache}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.ElementConnectionCache)
                {
                    elementConnectionCache = await ReliableDictionaryAccess<short, Dictionary<long, List<long>>>.Create(stateManager, ReliableDictionaryNames.ElementConnectionCache);
                    this.isElementConnectionCacheInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.ElementConnectionCache}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.EnergySourceCache)
                {
                    energySourceCache = await ReliableDictionaryAccess<short, List<long>>.Create(stateManager, ReliableDictionaryNames.EnergySourceCache);
                    this.isEnergySourceCacheInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.EnergySourceCache}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.RecloserCache)
                {
                    recloserCache = await ReliableDictionaryAccess<short, HashSet<long>>.Create(stateManager, ReliableDictionaryNames.RecloserCache);
                    this.isRecloserCacheInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.RecloserCache}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }

                if (AreReliableDictionariesInitialized)
                {
                    await ImportDataInCache();
                }
            }
        }

        public async Task<IModelDelta> ImportDataInCache()
        {
            while (!AreReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

			//Logger.LogVerbose($"{baseLogString} enter in ImportDataInCache.");

			IModelDelta modelDelta = await modelManager.TryGetAllModelEntitiesAsync();

            await ElementCache.SetAsync((short)TransactionFlag.NoTransaction, TransformDictionary(modelDelta.TopologyElements));
            await ElementConnectionCache.SetAsync((short)TransactionFlag.NoTransaction, modelDelta.ElementConnections);
            await EnergySourceCache.SetAsync((short)TransactionFlag.NoTransaction, modelDelta.EnergySources);
            await RecloserCache.SetAsync((short)TransactionFlag.NoTransaction, modelDelta.Reclosers);

            return modelDelta;
        }

		#region Model Provider
		public async Task<Dictionary<long, TopologyElement>> GetElementModels()
        {
            string verboseMessage = $"{baseLogString} entering GetElementModels method.";
            Logger.LogVerbose(verboseMessage);

            return await GetElementsFromCache(transactionFlag);
        }
        public async Task<Dictionary<long, List<long>>> GetConnections()
        {
            string verboseMessage = $"{baseLogString} entering GetConnections method.";
            Logger.LogVerbose(verboseMessage);

            return await GetConnectionsFromCache(transactionFlag);
        }
        public async Task<HashSet<long>> GetReclosers()
        {
            string verboseMessage = $"{baseLogString} entering GetReclosers method.";
            Logger.LogVerbose(verboseMessage);

            return await GetReclosersFromCache(transactionFlag);
        }
        public async Task<List<long>> GetEnergySources()
        {
            string verboseMessage = $"{baseLogString} entering GetEnergySources method.";
            Logger.LogVerbose(verboseMessage);

            return await GetEnergySourcesFromCache(transactionFlag);
        }
        public async Task<bool> IsRecloser(long recloserGid)
        {
            string verboseMessage = $"{baseLogString} entering IsRecloser method for element GID {recloserGid:X16}.";
            Logger.LogVerbose(verboseMessage);

            HashSet<long> reclosers = await GetReclosersFromCache(transactionFlag);

            return reclosers.Contains(recloserGid);
        }
		#endregion

		#region Distributed Transaction
		public async Task<bool> Prepare()
        {
            string verboseMessage = $"{baseLogString} entering PrepareForTransaction method.";
            Logger.LogVerbose(verboseMessage);

            bool measurementProviderTransaction = true;
            bool topologyProviderTransaction = true;

            bool success = true;
            try
            {
                Logger.LogDebug($"{baseLogString} PrepareForTransaction => Topology provider preparing for transaction.");

                Logger.LogDebug($"{baseLogString} PrepareForTransaction => Calling PrepareForTransaction on measurement provider.");
                measurementProviderTransaction = await measurementProviderClient.PrepareForTransaction();
                Logger.LogDebug($"{baseLogString} PrepareForTransaction => PrepareForTransaction from measurement provider returned success = {measurementProviderTransaction}.");

                transactionFlag = TransactionFlag.InTransaction;

                IModelDelta modelDelta = await modelManager.TryGetAllModelEntitiesAsync();

                Logger.LogDebug($"{baseLogString} PrepareForTransaction => Writting new data in cache under InTransaction flag.");
                await ElementCache.SetAsync((short)TransactionFlag.InTransaction, TransformDictionary(modelDelta.TopologyElements));
                await ElementConnectionCache.SetAsync((short)TransactionFlag.InTransaction, modelDelta.ElementConnections);
                await EnergySourceCache.SetAsync((short)TransactionFlag.InTransaction, modelDelta.EnergySources);
                await RecloserCache.SetAsync((short)TransactionFlag.InTransaction, modelDelta.Reclosers);
                Logger.LogDebug($"{baseLogString} PrepareForTransaction => All new data have been written in cache under InTransaction flag.");

                Logger.LogDebug($"{baseLogString} PrepareForTransaction => Calling PrepareForTransaction on topology provider.");
                //topologyProviderTransaction = await topologyProviderClient.PrepareForTransaction();
                await topologyProviderClient.PrepareForTransaction();
                Logger.LogDebug($"{baseLogString} PrepareForTransaction => PrepareForTransaction from topology provider returned success = {topologyProviderTransaction}.");

            }
            catch (Exception e)
            {
                Logger.LogFatal($"{baseLogString} PrepareForTransaction => Model provider failed to prepare for transaction." +
                    $"{Environment.NewLine} Exception message: {e.Message} " +
                    $"{Environment.NewLine} Stack trace: {e.StackTrace}");
                success = false;
            }

            return (success == true) ? topologyProviderTransaction && measurementProviderTransaction : false;
        }
        public async Task Commit()
        {
            string verboseMessage = $"{baseLogString} entering CommitTransaction method.";
            Logger.LogVerbose(verboseMessage);

            Logger.LogDebug($"{baseLogString} CommitTransaction => Getting InTransaction data from cache.");
            var elementModels = await GetElementsFromCache(TransactionFlag.InTransaction);
            var allElementConnections = await GetConnectionsFromCache(TransactionFlag.InTransaction);
            var energySources = await GetEnergySourcesFromCache(TransactionFlag.InTransaction);
            var reclosers = await GetReclosersFromCache(TransactionFlag.InTransaction);
            Logger.LogDebug($"{baseLogString} CommitTransaction => All InTransaction data have been retrieved from cache.");

            Logger.LogDebug($"{baseLogString} CommitTransaction => Writting new data in cache under NoTransaction flag.");
            await ElementCache.SetAsync((short)TransactionFlag.NoTransaction, elementModels);
            await ElementConnectionCache.SetAsync((short)TransactionFlag.NoTransaction, allElementConnections);
            await EnergySourceCache.SetAsync((short)TransactionFlag.NoTransaction, energySources);
            await RecloserCache.SetAsync((short)TransactionFlag.NoTransaction, reclosers);
            Logger.LogDebug($"{baseLogString} CommitTransaction => All new data have been written in cache under NoTransaction flag.");

            Logger.LogDebug($"{baseLogString} CommitTransaction => Calling CommitTransaction on measurement provider.");
            await measurementProviderClient.CommitTransaction();

            Logger.LogDebug($"{baseLogString} CommitTransaction => Calling CommitTransaction on topology provider.");
            await topologyProviderClient.CommitTransaction();

            transactionFlag = TransactionFlag.NoTransaction;
            logger.LogDebug("Model provider commited transaction successfully.");
        }
        public async Task Rollback()
        {
            string verboseMessage = $"{baseLogString} entering RollbackTransaction method.";
            Logger.LogVerbose(verboseMessage);

            Logger.LogDebug($"{baseLogString} RollbackTransaction => Removing data from cache under NoTransaction flag.");
            await ElementCache.TryRemoveAsync((short)TransactionFlag.InTransaction);
            await ElementConnectionCache.TryRemoveAsync((short)TransactionFlag.InTransaction);
            await EnergySourceCache.TryRemoveAsync((short)TransactionFlag.InTransaction);
            await RecloserCache.TryRemoveAsync((short)TransactionFlag.InTransaction);
            Logger.LogDebug($"{baseLogString} RollbackTransaction => All data from cache under NoTransaction flag have been removed.");

            Logger.LogDebug($"{baseLogString} RollbackTransaction => Calling RollbackTransaction on measurement provider.");
            await measurementProviderClient.RollbackTransaction();

            Logger.LogDebug($"{baseLogString} RollbackTransaction => Calling RollbackTransaction on topology provider.");
            await topologyProviderClient.RollbackTransaction();

            transactionFlag = TransactionFlag.NoTransaction;
            logger.LogDebug("Model provider rolled back successfully.");
        }
		#endregion

		#region CacheGetters
		private async Task<Dictionary<long, TopologyElement>> GetElementsFromCache(TransactionFlag forTransactionType)
        {
            string verboseMessage = $"{baseLogString} entering GetElementsFromCache method.";
            Logger.LogVerbose(verboseMessage);

            while (!AreReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            ConditionalValue<Dictionary<long, TopologyElement>> elements;

            if (await ElementCache.ContainsKeyAsync((short)forTransactionType))
            {
                elements = await ElementCache.TryGetValueAsync((short)forTransactionType);
            }
            else if (forTransactionType == TransactionFlag.NoTransaction)
            {
                IModelDelta newModelDelta = await ImportDataInCache();

                //await ElementCache.SetAsync((short)TransactionFlag.NoTransaction, newModelDelta.TopologyElements);

                return TransformDictionary(newModelDelta.TopologyElements);
            }
            else
            {
                string errorMessage = $"{baseLogString} GetElementsFromCache => Transaction flag is InTransaction, but there is no transaction model.";
                Logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            if (!elements.HasValue)
            {
                string errorMessage = $"{baseLogString} GetElementsFromCache => TryGetValueAsync() returns no value";
                Logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            return elements.Value;
        }
        private async Task<Dictionary<long, List<long>>> GetConnectionsFromCache(TransactionFlag forTransactionType)
        {
            string verboseMessage = $"{baseLogString} entering GetConnectionsFromCache method.";
            Logger.LogVerbose(verboseMessage);

            while (!AreReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            ConditionalValue<Dictionary<long, List<long>>> elementConnections;

            if (await ElementConnectionCache.ContainsKeyAsync((short)forTransactionType))
            {
                elementConnections = await ElementConnectionCache.TryGetValueAsync((short)forTransactionType);
            }
            else if (forTransactionType == TransactionFlag.NoTransaction)
            {
                IModelDelta newModelDelta = await ImportDataInCache();

                //await ElementConnectionCache.SetAsync((short)TransactionFlag.NoTransaction, newModelDelta.ElementConnections);

                return newModelDelta.ElementConnections;
            }
            else
            {
                string errorMessage = $"{baseLogString} GetConnectionsFromCache => Transaction flag is InTransaction, but there is no transaction model.";
                Logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            if (!elementConnections.HasValue)
            {
                string errorMessage = $"{baseLogString} GetConnectionsFromCache => TryGetValueAsync() returns no value";
                Logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            return elementConnections.Value;
        }
        private async Task<HashSet<long>> GetReclosersFromCache(TransactionFlag forTransactionType)
        {
            string verboseMessage = $"{baseLogString} entering GetReclosersFromCache method.";
            Logger.LogVerbose(verboseMessage);

            while (!AreReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            ConditionalValue<HashSet<long>> reclosers;

            if (await RecloserCache.ContainsKeyAsync((short)forTransactionType))
            {
                reclosers = await RecloserCache.TryGetValueAsync((short)forTransactionType);
            }
            else if (forTransactionType == TransactionFlag.NoTransaction)
            {
                IModelDelta newModelDelta = await ImportDataInCache();

                //await RecloserCache.SetAsync((short)TransactionFlag.NoTransaction, newModelDelta.Reclosers);

                return newModelDelta.Reclosers;
            }
            else
            {
                string errorMessage = $"{baseLogString} GetReclosersFromCache => Transaction flag is InTransaction, but there is no transaction model.";
                Logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            if (!reclosers.HasValue)
            {
                string errorMessage = $"{baseLogString} GetReclosersFromCache => TryGetValueAsync() returns no value";
                Logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            return reclosers.Value;
        }
        private async Task<List<long>> GetEnergySourcesFromCache(TransactionFlag forTransactionType)
        {
            string verboseMessage = $"{baseLogString} entering GetEnergySourcesFromCache method.";
            Logger.LogVerbose(verboseMessage);

            while (!AreReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            ConditionalValue<List<long>> energySources;

            if (await EnergySourceCache.ContainsKeyAsync((short)forTransactionType))
            {
                energySources = await EnergySourceCache.TryGetValueAsync((short)forTransactionType);
            }
            else if (forTransactionType == TransactionFlag.NoTransaction)
            {
                IModelDelta newModelDelta = await ImportDataInCache();

                //await EnergySourceCache.SetAsync((short)TransactionFlag.NoTransaction, newModelDelta.EnergySources);

                return newModelDelta.EnergySources;
            }
            else
            {
                string errorMessage = $"{baseLogString} GetEnergySourcesFromCache => Transaction flag is InTransaction, but there is no transaction model.";
                Logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            if (!energySources.HasValue)
            {
                string errorMessage = $"{baseLogString} GetEnergySourcesFromCache => TryGetValueAsync() returns no value";
                Logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            return energySources.Value;
        }
        #endregion

        private Dictionary<long, TopologyElement> TransformDictionary(Dictionary<long, ITopologyElement> dict)
        {
            Dictionary<long, TopologyElement> retVal = new Dictionary<long, TopologyElement>();

            foreach (var item in dict)
            {
                retVal.Add(item.Key, item.Value as TopologyElement);
            }

            return retVal;
        }
		//public void PrepareTopologyForTransaction()
		//{
		//    bool success;
		//    int numberOfTries = 0;
		//    do
		//    {
		//        numberOfTries++;
		//        success = Provider.Instance.TopologyProvider.PrepareForTransaction();
		//    } while (!success && numberOfTries < 3);

		//}

		//public void PrepareMeasurementForTransaction()
		//{
		//    bool success;
		//    int numberOfTries = 0;
		//    do
		//    {
		//        numberOfTries++;
		//        success = Provider.Instance.MeasurementProvider.PrepareForTransaction();
		//    } while (!success && numberOfTries < 3);
		//}

    }
}

using Common.CE;
using Common.CeContracts;
using Common.CeContracts.LoadFlow;
using Common.CeContracts.ModelProvider;
using Common.CeContracts.TopologyProvider;
using Common.PubSubContracts.DataContracts.CE;
using Common.PubSubContracts.DataContracts.CE.UIModels;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Names;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.PubSubContracts;
using OMS.Common.WcfClient.CE;
using OMS.Common.WcfClient.PubSub;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading.Tasks;

namespace CE.TopologyProviderImplementation
{
    public class TopologyProvider : ITopologyProviderContract
    {
        #region Fields
        private readonly string baseLogString;
        private readonly IReliableStateManager stateManager;
        private ConcurrentDictionary<long, int> recloserCounters;

        private const long topologyID = 1;
        private const long transactionTopologyID = 2;

        private TransactionFlag transactionFlag;
        private Task<bool> prepare;
        #endregion

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        #region Reliable Dictionaries
        private bool isTopologyCacheInitialized;
        private bool isTopologyCacheUIInitialized;
        private bool isTopologyCacheOMSInitialized;

        public bool AreDictionariesInitialized
        {
            get
            {
                return isTopologyCacheInitialized && isTopologyCacheOMSInitialized && isTopologyCacheUIInitialized;
            }
        }

        private ReliableDictionaryAccess<long, TopologyModel> topologyCache;
        public ReliableDictionaryAccess<long, TopologyModel> TopologyCache { get => topologyCache; }

        private ReliableDictionaryAccess<long, UIModel> topologyCacheUI;
        public ReliableDictionaryAccess<long, UIModel> TopologyCacheUI { get => topologyCacheUI; }

        private ReliableDictionaryAccess<long, OutageTopologyModel> topologyCacheOMS;
        public ReliableDictionaryAccess<long, OutageTopologyModel> TopologyCacheOMS { get => topologyCacheOMS; }

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
        }

        private async Task InitializeReliableCollections(NotifyStateManagerChangedEventArgs e)
        {
            if (e.Action == NotifyStateManagerChangedAction.Add)
            {
                var operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
                string reliableStateName = operation.ReliableState.Name.AbsolutePath;

                if (reliableStateName == ReliableDictionaryNames.TopologyCache)
                {
                    topologyCache = await ReliableDictionaryAccess<long, TopologyModel>.Create(stateManager, ReliableDictionaryNames.TopologyCache);
                    this.isTopologyCacheInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.TopologyCache}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.TopologyCacheUI)
                {
                    topologyCacheUI = await ReliableDictionaryAccess<long, UIModel>.Create(stateManager, ReliableDictionaryNames.TopologyCacheUI);
                    this.isTopologyCacheUIInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.TopologyCacheUI}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.TopologyCacheOMS)
                {
                    topologyCacheOMS = await ReliableDictionaryAccess<long, OutageTopologyModel>.Create(stateManager, ReliableDictionaryNames.TopologyCacheOMS);
                    this.isTopologyCacheOMSInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.TopologyCacheOMS}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
            }
        }
        #endregion Reliable Dictionaries

        public TopologyProvider(IReliableStateManager stateManager)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            string verboseMessage = $"{baseLogString} entering Ctor.";
            Logger.LogVerbose(verboseMessage);

            this.isTopologyCacheInitialized = false;
            this.isTopologyCacheUIInitialized = false;
            this.isTopologyCacheOMSInitialized = false;

            this.stateManager = stateManager;
            stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;

            recloserCounters = new ConcurrentDictionary<long, int>();

            transactionFlag = TransactionFlag.NoTransaction;

            string debugMessage = $"{baseLogString} Ctor => Clients initialized.";
            Logger.LogDebug(debugMessage);
        }

        private async Task<TopologyModel> GetTopologyFromCache(TopologyType type)
        {
            while (!AreDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            string verboseMessage = $"{baseLogString} entering GetTopologyFromCache method. Topology type {type}";
            Logger.LogVerbose(verboseMessage);
            ConditionalValue<TopologyModel> topology;

            long topologyID;
            if (type == TopologyType.NoSpecific)
            {
                if (transactionFlag == TransactionFlag.InTransaction)
                {
                    topologyID = 2;
                }
                else
                {
                    topologyID = 1;
                }
            }
            else
            {
                topologyID = (long)type;
            }

            if (await TopologyCache.ContainsKeyAsync(topologyID))
            {
                topology = await TopologyCache.TryGetValueAsync(topologyID);
            }
            else if (topologyID == (long)TopologyType.NonTransactionTopology)
            {
                //Logger.LogDebug($"{baseLogString} GetTopologyFromCache => Initializing topology.");
                //var newTopology = await CreateTopology("GetTopologyFromCache");
                //Logger.LogDebug($"{baseLogString} GetTopologyFromCache => Calling UpdateLoadFlow method from load flow client.");

                
                //Logger.LogDebug($"{baseLogString} GetTopologyFromCache => UpdateLoadFlow method from load flow client has been called successfully.");

                //await TopologyCache.SetAsync(topologyID, (TopologyModel)newTopology);
               
                //await RefreshOMSModel();
                //await RefreshUIModel();

                return new TopologyModel();
            }
            else
            {
                string errorMessage = $"{baseLogString} GetTopologyFromCache => TryGetValueAsync() There is no inTransaction topology.";
                Logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            if (!topology.HasValue)
            {
                string errorMessage = $"{baseLogString} GetTopologyFromCache => TryGetValueAsync() returns no value";
                Logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            return topology.Value as TopologyModel;
        }
        private async Task<TopologyModel> CreateTopology(string whoCalled)
        {
            string verboseMessage = $"{baseLogString} CreateTopology method called.";
            Logger.LogVerbose(verboseMessage);

            Logger.LogDebug($"{baseLogString} CreateTopology => Calling GetEnergySources method from model provider client.");
            var modelProviderClient = CeModelProviderClient.CreateClient();
            List<long> roots = await modelProviderClient.GetEnergySources();
            Logger.LogDebug($"{baseLogString} CreateTopology => GetEnergySources method from model provider client has been successfully called.");

            if (roots.Count == 0)
            {
                string message = $"{baseLogString} CreateTopology => GetEnergySources returned 0 energy sources.";
                Logger.LogWarning(message);
                //throw new Exception(message);
                return new TopologyModel();
            }
            else if(roots.Count > 1)
            {
                string message = $"{baseLogString} CreateTopology => GetEnergySources returned more then one energy sources. Will try to find source with GID != 0.";
                Logger.LogWarning(message);
            }

            long energySourceGid = 0;

            foreach (var source in roots)
            {
                if (source != 0)
                {
                    energySourceGid = source;
                    break;
                }
            }

            Logger.LogDebug($"{baseLogString} CreateTopology => Calling CreateGraphTopology method from topology builder client. Energy source with GID {energySourceGid:X16.}");
            var topologyBuilderClient = TopologyBuilderClient.CreateClient();
            var topology = await topologyBuilderClient.CreateGraphTopology(energySourceGid, $"Topology Provider => {whoCalled} => Create Topology");
            Logger.LogDebug($"{baseLogString} CreateTopology =>  CreateGraphTopology method from topology builder client has been successfully called.");


            if (topology == null)
            {
                string errorMessage = $"{baseLogString} CreateGraphTopology => CreateGraphTopology returned null.";
                Logger.LogError(errorMessage);
            }

            return topology;
        }

        public async Task DiscreteMeasurementDelegate()
        {
            string verboseMessage = $"{baseLogString} entering DiscreteMeasurementDelegate method.";
            Logger.LogVerbose(verboseMessage);

            var topology = await GetTopologyFromCache(TopologyType.NonTransactionTopology);
            var loadFlowClient = LoadFlowClient.CreateClient();
            topology = await loadFlowClient.UpdateLoadFlow(topology);

            await TopologyCache.SetAsync((short)TopologyType.NonTransactionTopology, (TopologyModel)topology);

            await RefreshUIModel();
            await RefreshOMSModel();
        }
        public async Task<TopologyModel> GetTopology()
        {
            string verboseMessage = $"{baseLogString} entering GetTopology method.";
            Logger.LogVerbose(verboseMessage);

            return await GetTopologyFromCache(TopologyType.NoSpecific);
        }
        public async Task<bool> IsElementRemote(long elementGid)
        {
            string verboseMessage = $"{baseLogString} entering IsElementRemote method. Element GID {elementGid:X16}.";
            Logger.LogVerbose(verboseMessage);

            bool isRemote;
            var topology = await GetTopologyFromCache(TopologyType.NoSpecific);

            if (topology.GetElementByGid(elementGid, out ITopologyElement element))
            {
                isRemote = element.IsRemote;
            }
            else
            {
                string errorMessage = $"{baseLogString} IsElementRemote => Element with GID {elementGid:X16} does not exist in Topology.";
                Logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            return isRemote;
        } 
        public async Task ResetRecloser(long recloserGid)
        {
            string verboseMessage = $"{baseLogString} entering ResetRecloser method. Recloser GID {recloserGid:X16}.";
            Logger.LogVerbose(verboseMessage);

            try
            {

                recloserCounters[recloserGid] = 0;
                                
                //var topology = await GetTopologyFromCache(TopologyType.NoSpecific);

                //if (topology.GetElementByGid(recloserGid, out ITopologyElement element))
                //{
                //    if (element is Recloser recloser)
                //    {
                //        var oldNumberOfTry = recloser.NumberOfTry;
                //        recloser.NumberOfTry = 0;
                //        await TopologyCache.SetAsync(topologyID, topology);

                //        Logger.LogDebug($"{baseLogString} ResetRecloser => RecloserGid: 0x{recloser.Id:X16}, IsActive: {recloser.IsActive}, NumberOfTry[OLD]: {oldNumberOfTry}, NumberOfTry[NEW]:{recloser.NumberOfTry}");
                //    }
                //    else
                //    {
                //        string errorMessage = $"{baseLogString} ResetRecloser => Element with GID {recloserGid:X16} is not a recloser.";
                //        Logger.LogError(errorMessage);
                //        throw new Exception(errorMessage);
                //    }
                //}
                //else
                //{
                //    string errorMessage = $"{baseLogString} ResetRecloser => Element with GID {recloserGid:X16} does not exist in Topology.";
                //    Logger.LogError(errorMessage);
                //    throw new Exception(errorMessage);
                //}

            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} ResetRecloser =>" +
                    $"{Environment.NewLine} Exception message: {e.Message} " +
                    $"{Environment.NewLine} Stack Trace: {e.StackTrace}";
                Logger.LogError(errorMessage);
                throw;
            }

        }

        public async Task<int> GetRecloserCount(long recloserGid)
        {
            if (recloserCounters.TryGetValue(recloserGid, out int count))
            {
                return count;
            }
            else
            {
                return 0;
            }
        }

        public async Task RecloserOpened(long recloserGid)
        {
            string verboseMessage = $"{baseLogString} entering ResetRecloser method. Recloser GID {recloserGid:X16}.";
            Logger.LogVerbose(verboseMessage);

            try
            {
                if (recloserCounters.TryGetValue(recloserGid, out int count))
                {
                    recloserCounters[recloserGid] = count+1;
                }
                else
                {
                    recloserCounters.TryAdd(recloserGid, 1);
                }

                //var topology = await GetTopologyFromCache(TopologyType.NoSpecific);

                //if (topology.GetElementByGid(recloserGid, out ITopologyElement element))
                //{
                //    if (element is Recloser recloser)
                //    {
                //        var oldNumberOfTry = recloser.NumberOfTry;
                //        recloser.NumberOfTry++;
                //        await TopologyCache.SetAsync(topologyID, topology);

                //        Logger.LogDebug($"{baseLogString} RecloserOpened => RecloserGid: 0x{recloser.Id:X16}, IsActive: {recloser.IsActive}, NumberOfTry[OLD]: {oldNumberOfTry}, NumberOfTry[NEW]:{recloser.NumberOfTry}");
                //    }
                //    else
                //    {
                //        string errorMessage = $"{baseLogString} RecloserOpened => Element with GID {recloserGid:X16} is not a recloser.";
                //        Logger.LogError(errorMessage);
                //        throw new Exception(errorMessage);
                //    }
                //}
                //else
                //{
                //    string errorMessage = $"{baseLogString} RecloserOpened => Element with GID {recloserGid:X16} does not exist in Topology.";
                //    Logger.LogError(errorMessage);
                //    throw new Exception(errorMessage);
                //}

            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} RecloserOpened =>" +
                    $"{Environment.NewLine} Exception message: {e.Message} " +
                    $"{Environment.NewLine} Stack Trace: {e.StackTrace}";
                Logger.LogError(errorMessage);
                throw;
            }
        }

        #region Distributed Transaction
        public async Task<bool> PrepareForTransaction()
        {
            string verboseMessage = $"{baseLogString} entering PrepareForTransaction method.";
            Logger.LogVerbose(verboseMessage);

            bool success = true;

            try
            {
                Logger.LogDebug($"{baseLogString} PrepareForTransaction => Creating new transaction topology.");
                var newTopology = await CreateTopology("PrepareForTransaction");

                //TODO: (look) prebaceno u commit
                //Logger.LogDebug($"{baseLogString} CommitTransaction => Calling UpdateLoadFlow method from load flow client.");
                //var loadFlowClient = LoadFlowClient.CreateClient();
                //var topology = await loadFlowClient.UpdateLoadFlow(newTopology);
                //Logger.LogDebug($"{baseLogString} CommitTransaction => UpdateLoadFlow method from load flow client has been called successfully.");

                Logger.LogDebug($"{baseLogString} PrepareForTransaction => Writting new transaction topology into cache.");
                await TopologyCache.SetAsync(transactionTopologyID, (TopologyModel)newTopology);

                transactionFlag = TransactionFlag.InTransaction;

                //TODO: (look) u vezi prebacivanja u commit
                //await TopologyCache.SetAsync(transactionTopologyID, (TopologyModel)topology); //inace, sadrzaj topology bi uvek bio prepisan preko newTopology, jer idu na isti kljuc
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} PrepareForTransaction => Exception message: {e.Message} {Environment.NewLine} Stack trace: {e.StackTrace}.";
                Logger.LogError(errorMessage);
                success = false;
            }

            return success;
        }

        public async Task CommitTransaction()
        {
            string verboseMessage = $"{baseLogString} entering CommitTransaction method.";
            Logger.LogVerbose(verboseMessage);

            Logger.LogDebug($"{baseLogString} CommitTransaction => Getting topology from cache.");
            var topology = await GetTopologyFromCache(TopologyType.TransactionTopology);
            Logger.LogDebug($"{baseLogString} CommitTransaction => Getting topology from cache successfully ended.");

            await TopologyCache.SetAsync(topologyID, (TopologyModel)topology);

            //TODO: Daje se vremena skadi da zavrsi svoj commit. FIND a better solution!
            await Task.Delay(10_000);

            Logger.LogDebug($"{baseLogString} CommitTransaction => Calling UpdateLoadFlow method from load flow client.");
            var loadFlowClient = LoadFlowClient.CreateClient();
            topology = await loadFlowClient.UpdateLoadFlow(topology);
            Logger.LogDebug($"{baseLogString} CommitTransaction => UpdateLoadFlow method from load flow client has been called successfully.");

            if (topology == null)
            {
                string errorMessage = $"{baseLogString} CommitTransaction => Load flow returned null.";
                Logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            await TopologyCache.SetAsync(topologyID, (TopologyModel)topology); //sadrzaj topology, ce zameniti prethodno uneti (takodje u ovoj metodi)

            //await TopologyCache.SetAsync(topologyID, (TopologyModel)topology);
            //await TopologyCache.SetAsync(transactionTopologyID, (TopologyModel)topology);
            transactionFlag = TransactionFlag.NoTransaction;

            await RefreshOMSModel();
            await RefreshUIModel();

            //ProviderTopologyConnectionDelegate?.Invoke(Topology);
        }

        public async Task RollbackTransaction()
        {
            string verboseMessage = $"{baseLogString} entering RollbackTransaction method.";
            Logger.LogVerbose(verboseMessage);

            var topology = await GetTopologyFromCache(TopologyType.NonTransactionTopology);
            await TopologyCache.SetAsync((long)TopologyType.TransactionTopology, (TopologyModel)topology);
            
            transactionFlag = TransactionFlag.NoTransaction;
        }
        #endregion

        public async Task<OutageTopologyModel> GetOMSModel()
        {
            while (!AreDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            ConditionalValue<OutageTopologyModel>  omsModel;

            if (await TopologyCacheOMS.ContainsKeyAsync(1))
            {
                omsModel = await TopologyCacheOMS.TryGetValueAsync(1);
            }
            else
            {
                return await RefreshOMSModel();
            }

            if (!omsModel.HasValue)
            {
                string errorMessage = $"{baseLogString} GetOMSModel => TryGetValueAsync() returns no value";
                Logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            return omsModel.Value as OutageTopologyModel;
        }
        public async Task<UIModel> GetUIModel()
        {
            while (!AreDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            ConditionalValue<UIModel> uiModel;

            if (await TopologyCacheUI.ContainsKeyAsync(1))
            {
                uiModel = await TopologyCacheUI.TryGetValueAsync(1);
            }
            else
            {
                return await RefreshUIModel();
            }

            if (!uiModel.HasValue)
            {
                string errorMessage = $"{baseLogString} GetUIModel => TryGetValueAsync() returns no value";
                Logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            return uiModel.Value as UIModel;
        }
        private async Task<UIModel> RefreshUIModel()
        {
            var topology = await GetTopologyFromCache(TopologyType.NoSpecific);
            var topologyConverterClient = TopologyConverterClient.CreateClient();
            UIModel newUIModel = await topologyConverterClient.ConvertTopologyToUIModel(topology);
            await TopologyCacheUI.SetAsync(1, (UIModel)newUIModel);

            await PublishUIModel(newUIModel);

            return newUIModel;
        }
        private async Task<OutageTopologyModel> RefreshOMSModel()
        {
            var topology = await GetTopologyFromCache(TopologyType.NoSpecific);
            var topologyConverterClient = TopologyConverterClient.CreateClient();
            var newOmsModel = await topologyConverterClient.ConvertTopologyToOMSModel(topology);
            await TopologyCacheOMS.SetAsync(1, (OutageTopologyModel)newOmsModel);

            await PublishOMSModel(newOmsModel);

            return newOmsModel;
        }

        #region Publisher
        public async Task PublishOMSModel(OutageTopologyModel outageTopologyModel)
        {
            string verboseMessage = $"{baseLogString} entering PublishOMSModel method.";
            Logger.LogVerbose(verboseMessage);

            OMSModelMessage message = new OMSModelMessage(outageTopologyModel);
            CalculationEnginePublication publication = new CalculationEnginePublication(Topic.OMS_MODEL, message);
            try
            {
                var publisherClient = PublisherClient.CreateClient();
                await publisherClient.Publish(publication, MicroserviceNames.CeTopologyProviderService);
                Logger.LogInformation($"{baseLogString} PublishOMSModel => Topology provider service published data of topic: {publication.Topic}");
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} PublishOMSModel => Failed to publish OMS Model. " +
                    $"{Environment.NewLine}Exception message {e.Message}. " +
                    $"{Environment.NewLine} Stack trace: {e.StackTrace}";
                Logger.LogError(errorMessage, e);

                await Task.Delay(2000);

                await PublishOMSModel(outageTopologyModel);
            }
        }
        public async Task PublishUIModel(UIModel uiTopologyModel)
        {
            string verboseMessage = $"{baseLogString} entering PublishUIModel method.";
            Logger.LogVerbose(verboseMessage);

            TopologyForUIMessage message = new TopologyForUIMessage(uiTopologyModel);
            CalculationEnginePublication publication = new CalculationEnginePublication(Topic.TOPOLOGY, message);
            try
            {
                var publisherClient = PublisherClient.CreateClient();
                await publisherClient.Publish(publication, MicroserviceNames.CeTopologyProviderService);
                Logger.LogInformation($"{baseLogString} PublishUIModel => Topology provider service published data of topic: {publication.Topic}");
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} PublishUIModel => Failed to publish UI Model. " +
                    $"{Environment.NewLine}Exception message {e.Message}. " +
                    $"{Environment.NewLine} Stack trace: {e.StackTrace}";
                Logger.LogError(errorMessage, e);

                await Task.Delay(2000);

                await PublishUIModel(uiTopologyModel);
            }
        }

        public Task<bool> IsAlive()
        {
            return Task.Run(() => { return true; });
        }
        #endregion
    }

	enum TopologyType
    {
        NonTransactionTopology = 1,
        TransactionTopology,
        NoSpecific
    }
}

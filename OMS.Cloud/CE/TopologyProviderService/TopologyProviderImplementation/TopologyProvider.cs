using Common.CE;
using Common.CE.Interfaces;
using Common.CeContracts;
using Common.CeContracts.TopologyProvider;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Names;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.PubSub;
using OMS.Common.PubSubContracts;
using OMS.Common.WcfClient.CE;
using OMS.Common.WcfClient.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ReliableDictionaryNames = Common.CE.ReliableDictionaryNames;

namespace TopologyProviderImplementation
{
	public class TopologyProvider : ITopologyProviderContract
    {
        #region Fields
        private readonly long topologyID = 1;
        private readonly long transactionTopologyID = 2;
        private TransactionFlag transactionFlag;
        #endregion

        private readonly string baseLogString;
        private readonly IReliableStateManager stateManager;

        private readonly IPublisherContract publisherClient;
        private readonly ModelProviderClient modelProviderClient;
        private readonly LoadFlowClient loadFlowClient;
        private readonly TopologyBuilderClient topologyBuilderClient;
        private readonly TopologyConverterClient topologyConverterClient;

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        private bool isTopologyCacheInitialized;
        private bool isTopologyCacheUIInitialized;
        private bool isTopologyCacheOMSInitialized;

        private ReliableDictionaryAccess<long, ITopology> topologyCache;
        public ReliableDictionaryAccess<long, ITopology> TopologyCache { get => topologyCache; }

        private ReliableDictionaryAccess<long, UIModel> topologyCacheUI;
        public ReliableDictionaryAccess<long, UIModel> TopologyCacheUI { get => topologyCacheUI; }

        private ReliableDictionaryAccess<long, IOutageTopologyModel> topologyCacheOMS;
        public ReliableDictionaryAccess<long, IOutageTopologyModel> TopologyCacheOMS { get => topologyCacheOMS; }

        public TopologyProvider(IReliableStateManager stateManager)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            string verboseMessage = $"{baseLogString} entering Ctor.";
            Logger.LogVerbose(verboseMessage);

            this.modelProviderClient = ModelProviderClient.CreateClient();
            this.loadFlowClient = LoadFlowClient.CreateClient();
            this.topologyBuilderClient = TopologyBuilderClient.CreateClient();
            this.topologyConverterClient = TopologyConverterClient.CreateClient();
            this.publisherClient = PublisherClient.CreateClient();

            this.stateManager = stateManager;
            stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;

            this.isTopologyCacheInitialized = false;
            this.isTopologyCacheUIInitialized = false;
            this.isTopologyCacheOMSInitialized = false;

            transactionFlag = TransactionFlag.NoTransaction;

            string debugMessage = $"{baseLogString} Ctor => Clients initialized.";
            Logger.LogDebug(debugMessage);
        }
        private async void OnStateManagerChangedHandler(object sender, NotifyStateManagerChangedEventArgs e)
        {
            if (e.Action == NotifyStateManagerChangedAction.Add)
            {
                var operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
                string reliableStateName = operation.ReliableState.Name.AbsolutePath;

                if (reliableStateName == ReliableDictionaryNames.TopologyCache)
                {
                    topologyCache = await ReliableDictionaryAccess<long, ITopology>.Create(stateManager, ReliableDictionaryNames.TopologyCache);
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
                    topologyCacheOMS = await ReliableDictionaryAccess<long, IOutageTopologyModel>.Create(stateManager, ReliableDictionaryNames.TopologyCacheOMS);
                    this.isTopologyCacheOMSInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.TopologyCacheOMS}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
            }
        }
        private async Task<ITopology> GetTopologyFromCache(TopologyType type)
        {
            string verboseMessage = $"{baseLogString} entering GetTopologyFromCache method. Topology type {type}";
            Logger.LogVerbose(verboseMessage);
            ConditionalValue<ITopology> topology;

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
                Logger.LogDebug($"{baseLogString} GetTopologyFromCache => Initializing topology.");
                ITopology newTopology = await CreateTopology();

                Logger.LogDebug($"{baseLogString} PrepareForTransaction => Calling UpdateLoadFlow method from load flow client.");
                newTopology = await loadFlowClient.UpdateLoadFlow(newTopology);
                Logger.LogDebug($"{baseLogString} PrepareForTransaction => UpdateLoadFlow method from load flow client has been called successfully.");

                await TopologyCache.SetAsync(topologyID, newTopology);

                await RefreshOMSModel();
                await RefreshUIModel();

                return newTopology;
            }
            else
            {
                return null;
            }

            if (topology.HasValue)
            {
                string errorMessage = $"{baseLogString} GetTopologyFromCache => TryGetValueAsync() returns no value";
                Logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            return topology.Value as ITopology;
        }
        private async Task<ITopology> CreateTopology()
        {
            string verboseMessage = $"{baseLogString} CreateTopology method called.";
            Logger.LogVerbose(verboseMessage);

            Logger.LogDebug($"{baseLogString} CreateTopology => Calling GetEnergySources method from model provider client.");
            List<long> roots = await modelProviderClient.GetEnergySources();
            Logger.LogDebug($"{baseLogString} CreateTopology => GetEnergySources method from model provider client has been successfully called.");

            if (roots.Count == 0)
            {
                string message = $"{baseLogString} CreateTopology => GetEnergySources returned 0 energy sources.";
                Logger.LogError(message);
                throw new Exception(message);
            }
            else if(roots.Count > 1)
            {
                string message = $"{baseLogString} CreateTopology => GetEnergySources returned more then one energy sources. First will be considered.";
                Logger.LogError(message);
            }

            long energySourceGid = roots.First();

            Logger.LogDebug($"{baseLogString} CreateTopology => Calling CreateGraphTopology method from topology builder client. Energy source with GID {energySourceGid:X16.}");
            ITopology topology = await topologyBuilderClient.CreateGraphTopology(energySourceGid);
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
            topology = await loadFlowClient.UpdateLoadFlow(topology);

            await TopologyCache.SetAsync((short)TopologyType.NonTransactionTopology, topology);

            await RefreshUIModel();
            await RefreshOMSModel();
        }
        public async Task<ITopology> GetTopology()
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
            ITopology topology = await GetTopologyFromCache(TopologyType.NoSpecific);

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
                ITopology topology = await GetTopologyFromCache(TopologyType.NoSpecific);

                if (topology.GetElementByGid(recloserGid, out ITopologyElement element))
                {
                    if (element is Recloser recloser)
                    {
                        recloser.NumberOfTry = 0;
                    }
                    else
                    {
                        string errorMessage = $"{baseLogString} ResetRecloser => Element with GID {recloserGid:X16} is not a recloser.";
                        Logger.LogError(errorMessage);
                        throw new Exception(errorMessage);
                    }
                }
                else
                {
                    string errorMessage = $"{baseLogString} ResetRecloser => Element with GID {recloserGid:X16} does not exist in Topology.";
                    Logger.LogError(errorMessage);
                    throw new Exception(errorMessage);
                }

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

        #region Distributed Transaction
        public async Task CommitTransaction()
        {
            string verboseMessage = $"{baseLogString} entering CommitTransaction method.";
            Logger.LogVerbose(verboseMessage);

            ITopology topology = await GetTopologyFromCache(TopologyType.TransactionTopology);

            Logger.LogDebug($"{baseLogString} PrepareForTransaction => Calling UpdateLoadFlow method from load flow client.");
            topology = await loadFlowClient.UpdateLoadFlow(topology);
            Logger.LogDebug($"{baseLogString} PrepareForTransaction => UpdateLoadFlow method from load flow client has been called successfully.");

            if (topology == null)
            {
                string errorMessage = $"{baseLogString} CommitTransaction => Load flow returned null.";
                Logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            await TopologyCache.SetAsync(topologyID, topology);
            await TopologyCache.SetAsync(transactionTopologyID, topology);

            await RefreshOMSModel();
            await RefreshUIModel();

           //ProviderTopologyConnectionDelegate?.Invoke(Topology);
            transactionFlag = TransactionFlag.NoTransaction;
        }
        public async Task<bool> PrepareForTransaction()
        {
            string verboseMessage = $"{baseLogString} entering PrepareForTransaction method.";
            Logger.LogVerbose(verboseMessage);

            bool success = true;

            try
            {
                Logger.LogDebug($"{baseLogString} PrepareForTransaction => Creating new transaction topology.");
                ITopology newTopology = await CreateTopology();

                Logger.LogDebug($"{baseLogString} PrepareForTransaction => Writting new transaction topology into cache.");
                await TopologyCache.SetAsync(transactionTopologyID, newTopology);

                transactionFlag = TransactionFlag.InTransaction;
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} PrepareForTransaction => Exception message: {e.Message} {Environment.NewLine} Stack trace: {e.StackTrace}.";
                Logger.LogError(errorMessage);
                success = false;
            }

            return success;
        }
        public async Task RollbackTransaction()
        {
            string verboseMessage = $"{baseLogString} entering RollbackTransaction method.";
            Logger.LogVerbose(verboseMessage);

            ITopology topology = await GetTopologyFromCache(TopologyType.NonTransactionTopology);
            await TopologyCache.SetAsync((long)TopologyType.TransactionTopology, topology);
            
            transactionFlag = TransactionFlag.NoTransaction;
        }
        #endregion

        public async Task<IOutageTopologyModel> GetOMSModel()
        {
            ConditionalValue<IOutageTopologyModel>  omsModel;

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

            return omsModel.Value as IOutageTopologyModel;
        }
        public async Task<UIModel> GetUIModel()
        {
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
            ITopology topology = await GetTopologyFromCache(TopologyType.NoSpecific);
            UIModel newUIModel = await topologyConverterClient.ConvertTopologyToUIModel(topology);
            await TopologyCacheUI.SetAsync(1, newUIModel);

            await PublishUIModel(newUIModel);

            return newUIModel;
        }
        private async Task<IOutageTopologyModel> RefreshOMSModel()
        {
            ITopology topology = await GetTopologyFromCache(TopologyType.NoSpecific);
            IOutageTopologyModel newOmsModel = await topologyConverterClient.ConvertTopologyToOMSModel(topology);
            await TopologyCacheOMS.SetAsync(1, newOmsModel);

            await PublishOMSModel(newOmsModel);

            return newOmsModel;
        }

        #region Publisher
        public async Task PublishOMSModel(IOutageTopologyModel outageTopologyModel)
        {
            string verboseMessage = $"{baseLogString} entering PublishOMSModel method.";
            Logger.LogVerbose(verboseMessage);

            OMSModelMessage message = new OMSModelMessage(outageTopologyModel);
            CalculationEnginePublication publication = new CalculationEnginePublication(Topic.OMS_MODEL, message);
            try
            {
                await publisherClient.Publish(publication, MicroserviceNames.TopologyProviderService);
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
                await publisherClient.Publish(publication, MicroserviceNames.TopologyProviderService);
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
        #endregion
    }

	enum TopologyType
    {
        NonTransactionTopology = 1,
        TransactionTopology,
        NoSpecific
    }
}
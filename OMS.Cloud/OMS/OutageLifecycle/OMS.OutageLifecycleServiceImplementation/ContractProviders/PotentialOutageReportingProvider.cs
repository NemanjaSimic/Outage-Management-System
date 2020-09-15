using Common.OMS;
using Common.OmsContracts.DataContracts.OutageDatabaseModel;
using Common.OmsContracts.HistoryDBManager;
using Common.OmsContracts.ModelAccess;
using Common.OmsContracts.ModelProvider;
using Common.OmsContracts.OutageLifecycle;
using Common.PubSubContracts.DataContracts.CE;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.WcfClient.CE;
using OMS.Common.WcfClient.OMS.HistoryDBManager;
using OMS.Common.WcfClient.OMS.ModelAccess;
using OMS.Common.WcfClient.OMS.ModelProvider;
using OMS.OutageLifecycleImplementation.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OMS.OutageLifecycleImplementation.ContractProviders
{
    public class PotentialOutageReportingProvider : IPotentialOutageReportingContract
    {
        private readonly string baseLogString;
        private readonly OutageLifecycleHelper lifecycleHelper;
        private readonly OutageMessageMapper outageMessageMapper;
        private readonly HashSet<CommandOriginType> ignorableCommandOriginTypes;
        private readonly IReliableStateManager stateManager;

		private ICloudLogger logger;

        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

		#region Reliable Dictionaries
		private bool isRecloserOutageMapInitialized;
        private bool isOutageTopologyModelInitialized;

        private bool ReliableDictionariesInitialized
		{
			get
			{
				return isRecloserOutageMapInitialized &&
                       isOutageTopologyModelInitialized;
			}
		}

        private ReliableDictionaryAccess<long, Dictionary<long, List<long>>> recloserOutageMap;
        private ReliableDictionaryAccess<long, Dictionary<long, List<long>>> RecloserOutageMap
        {
            get { return recloserOutageMap; }
        }

        private ReliableDictionaryAccess<string, OutageTopologyModel> outageTopologyModel;
        private ReliableDictionaryAccess<string, OutageTopologyModel> OutageTopologyModel
        {
            get { return outageTopologyModel; }
        }

        private async void OnStateManagerChangedHandler(object sender, NotifyStateManagerChangedEventArgs e)
		{
			if (e.Action == NotifyStateManagerChangedAction.Add)
			{
				var operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
				string reliableStateName = operation.ReliableState.Name.AbsolutePath;

                if (reliableStateName == ReliableDictionaryNames.RecloserOutageMap)
                {
                    this.recloserOutageMap = await ReliableDictionaryAccess<long, Dictionary<long, List<long>>>.Create(stateManager, ReliableDictionaryNames.RecloserOutageMap);
                    this.isRecloserOutageMapInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.RecloserOutageMap}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.OutageTopologyModel)
                {
                    this.outageTopologyModel = await ReliableDictionaryAccess<string, OutageTopologyModel>.Create(stateManager, ReliableDictionaryNames.OutageTopologyModel);
                    this.isOutageTopologyModelInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.OutageTopologyModel}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
            }
		}
		#endregion Reliable Dictionaries

		public PotentialOutageReportingProvider(IReliableStateManager stateManager, OutageLifecycleHelper outageLifecycleHelper)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

			this.outageMessageMapper = new OutageMessageMapper();
            this.lifecycleHelper = outageLifecycleHelper;

            this.ignorableCommandOriginTypes = new HashSet<CommandOriginType>()
            {
                CommandOriginType.USER_COMMAND,
                CommandOriginType.ISOLATING_ALGORITHM_COMMAND,
                CommandOriginType.UNKNOWN_ORIGIN,
            };

			this.isRecloserOutageMapInitialized = false;
            this.isOutageTopologyModelInitialized = false;

            this.stateManager = stateManager;
			this.stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;
		}

        #region IPotentialOutageReportingContract
        public async Task<bool> ReportPotentialOutage(long elementGid, CommandOriginType commandOriginType)
        {
            Logger.LogVerbose($"{baseLogString} ReportPotentialOutage method started. ElementGid: 0x{elementGid:X16}, CommandOriginType: {commandOriginType}");

            while(!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            try
            {
                #region Preconditions
                var ceModelProviderClient = CeModelProviderClient.CreateClient();
                if (await ceModelProviderClient.IsRecloser(elementGid))
                {
                    Logger.LogWarning($"{baseLogString} ReportPotentialOutage => Element with gid 0x{elementGid:X16} is a Recloser. Call to ReportPotentialOutage aborted.");
                    return false;
                }

                var enumerableTopology = await OutageTopologyModel.GetEnumerableDictionaryAsync();

                if (!enumerableTopology.ContainsKey(ReliableDictionaryNames.OutageTopologyModel))
                {
                    Logger.LogError($"{baseLogString} Start => Topology not found in Rel Dictionary: {ReliableDictionaryNames.OutageTopologyModel}.");
                    return false;
                }

                var topology = enumerableTopology[ReliableDictionaryNames.OutageTopologyModel];
                var affectedConsumersGids = lifecycleHelper.GetAffectedConsumers(elementGid, topology);

                var historyDBManagerClient = HistoryDBManagerClient.CreateClient();
                var outageModelReadAccessClient = OutageModelReadAccessClient.CreateClient();

                if (!(await CheckPreconditions(elementGid, commandOriginType, affectedConsumersGids, outageModelReadAccessClient, historyDBManagerClient)))
                {
                    Logger.LogError($"{baseLogString} ReportPotentialOutage => Parameters do not satisfy required preconditions. OutageId: {elementGid}, CommandOriginType: {commandOriginType}");
                    return false;
                }
                #endregion Preconditions

                Logger.LogInformation($"{baseLogString} ReportPotentialOutage => Reporting outage for gid: 0x{elementGid:X16}, CommandOriginType: {commandOriginType}");

                var result = await StoreActiveOutage(elementGid, affectedConsumersGids, topology);

                if (!result.HasValue)
                {
                    Logger.LogError($"{baseLogString} ReportPotentialOutage => Storing outage on element 0x{elementGid:X16} FAILED.");
                    return false;
                }

                var createdOutage = result.Value;
                Logger.LogInformation($"{baseLogString} ReportPotentialOutage => Outage on element with gid: 0x{createdOutage.OutageElementGid:x16} is successfully stored in database.");

                await historyDBManagerClient.OnSwitchOpened(elementGid, createdOutage.OutageId);
                await historyDBManagerClient.OnConsumerBlackedOut(affectedConsumersGids, createdOutage.OutageId);

                return await lifecycleHelper.PublishOutageAsync(Topic.ACTIVE_OUTAGE, outageMessageMapper.MapOutageEntity(createdOutage));
            }
            catch (Exception e)
            {
                string message = $"{baseLogString} ReportPotentialOutage =>  exception: {e.Message}";
                Logger.LogError(message, e);
                return false;
            }
        }

        public Task<bool> IsAlive()
        {
            return Task.Run(() => { return true; });
        }
        #endregion IPotentialOutageReportingContract

        #region Private Methods
        private async Task OnZeroAffectedConsumersCase(long elementGid, IHistoryDBManagerContract historyDBManagerClient)
        {
            bool isSwitchInvoked = false;

            var enumerableRecloserOutageMap = await RecloserOutageMap.GetEnumerableDictionaryAsync();

            if (enumerableRecloserOutageMap.TryGetValue(elementGid, out Dictionary<long, List<long>> outageAffectedPair))
            {
                foreach (var pair in outageAffectedPair)
                {
                    await historyDBManagerClient.OnConsumerBlackedOut(pair.Value, pair.Key);
                    await historyDBManagerClient.OnSwitchOpened(elementGid, pair.Key);
                    isSwitchInvoked = true;
                }
            }

            if (!isSwitchInvoked)
            {
                await historyDBManagerClient.OnSwitchOpened(elementGid, null);
            }
        }

        private async Task<bool> CheckPreconditions(long elementGid, CommandOriginType commandOriginType, List<long> affectedConsumersGids, IOutageModelReadAccessContract outageModelReadAccessClient, IHistoryDBManagerContract historyDBManagerClient)
        {
            if (this.ignorableCommandOriginTypes.Contains(commandOriginType))
            {
                Logger.LogDebug($"{baseLogString} CheckPreconditions => ignorable command origin type: {commandOriginType}");
                return false;
            }

            var commandedElements = await outageModelReadAccessClient.GetCommandedElements();
            var optimumIsolationPoints = await outageModelReadAccessClient.GetOptimumIsolatioPoints();

            if (commandedElements.ContainsKey(elementGid) || optimumIsolationPoints.ContainsKey(elementGid))
            {
                await historyDBManagerClient.OnSwitchOpened(elementGid, null);
                await historyDBManagerClient.OnConsumerBlackedOut(affectedConsumersGids, null);

                Logger.LogWarning($"{baseLogString} CheckPreconditions => ElementGid 0x{elementGid:X16} not found in commandedElements or optimumIsolationPoints.");
                return false;
            }

            if (affectedConsumersGids.Count == 0)
            {
                await OnZeroAffectedConsumersCase(elementGid, historyDBManagerClient);

                Logger.LogError($"{baseLogString} ReportPotentialOutage => There is no affected consumers => outage report is not valid. ElementGid: 0x{elementGid:X16}, CommandOriginType: {commandOriginType}");
                return false;
            }

            return true;
        }

        private async Task<ConditionalValue<OutageEntity>> StoreActiveOutage(long elementGid, List<long> affectedConsumersGids, OutageTopologyModel topology)
        {
            //var expression = new OutageExpression()
            //{
            //    Predicate = o => o.OutageElementGid == elementGid && o.OutageState != OutageState.ARCHIVED,
            //};

            //var outages = await outageModelAccessClient.FindOutage(expression);

            var outageModelAccessClient = OutageModelAccessClient.CreateClient();
            var allOutages = await outageModelAccessClient.GetAllOutages();
            var targetedOutages = allOutages.Where(outage => outage.OutageElementGid == elementGid && outage.OutageState != OutageState.ARCHIVED);

            if (targetedOutages.FirstOrDefault() != null)
            {
                Logger.LogWarning($"{baseLogString} StoreActiveOutage => Malfunction on element with gid: 0x{elementGid:x16} has already been reported.");
                return new ConditionalValue<OutageEntity>(false, null);
            }

            List<Consumer> consumerDbEntities = lifecycleHelper.GetAffectedConsumersFromDatabase(affectedConsumersGids);

            if (consumerDbEntities.Count != affectedConsumersGids.Count)
            {
                Logger.LogWarning($"{baseLogString} StoreActiveOutage => Some of affected consumers are not present in database.");
                return new ConditionalValue<OutageEntity>(false, null);
            }

            long recloserId = lifecycleHelper.GetRecloserForHeadBreaker(elementGid, topology);

            List<Equipment> defaultIsolationPoints = await lifecycleHelper.GetEquipmentEntityAsync(new List<long> { elementGid, recloserId });

            OutageEntity createdActiveOutage = new OutageEntity
            {
                AffectedConsumers = consumerDbEntities,
                OutageState = OutageState.CREATED,
                ReportTime = DateTime.UtcNow,
                DefaultIsolationPoints = defaultIsolationPoints,
            };

            var activeOutageDbEntity = await outageModelAccessClient.AddOutage(createdActiveOutage);

            if (activeOutageDbEntity == null)
            {
                Logger.LogError($"{baseLogString} StoreActiveOutage => activeOutageDbEntity is null.");
                return new ConditionalValue<OutageEntity>(false, null);
            }

            await UpdateRecloserOutageMap(recloserId, affectedConsumersGids, activeOutageDbEntity);

            return new ConditionalValue<OutageEntity>(true, activeOutageDbEntity);
        }

        private async Task UpdateRecloserOutageMap(long recloserId, List<long> affectedConsumersGids, OutageEntity createdActiveOutage)
        {
            var enumerableRecloserOutageMap = await RecloserOutageMap.GetEnumerableDictionaryAsync();

            if (enumerableRecloserOutageMap.TryGetValue(recloserId, out Dictionary<long, List<long>> outageToAffectedConsumersMap))
            {
                if (outageToAffectedConsumersMap.ContainsKey(createdActiveOutage.OutageId))
                {
                    outageToAffectedConsumersMap[createdActiveOutage.OutageId] = new List<long>(affectedConsumersGids);
                }
                else
                {
                    outageToAffectedConsumersMap.Add(createdActiveOutage.OutageId, affectedConsumersGids);
                }

                await RecloserOutageMap.SetAsync(recloserId, outageToAffectedConsumersMap);
            }
            else
            {
                var outageIdToAffectedConsumersMap = new Dictionary<long, List<long>>()
                {
                    { createdActiveOutage.OutageId, affectedConsumersGids }
                };

                await RecloserOutageMap.SetAsync(recloserId, outageIdToAffectedConsumersMap);
            }
        }
        #endregion Private Methods
    }
}

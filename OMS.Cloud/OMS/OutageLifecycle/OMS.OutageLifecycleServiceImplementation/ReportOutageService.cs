using Common.OMS;
using Common.OmsContracts.OutageLifecycle;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.WcfClient.OMS;
using OutageDatabase.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.OMS.OutageDatabaseModel;
using OMS.OutageLifecycleServiceImplementation.OutageLCHelper;
using Common.CE;
using OMS.Common.PubSub;
using Common.OmsContracts.ModelProvider;
using Common.OmsContracts.HistoryDBManager;

namespace OMS.OutageLifecycleServiceImplementation
{
	public class ReportOutageService : IReportOutageContract
	{
		private UnitOfWork dbContext;
        private OutageLifecycleHelper outageLifecycleHelper;
		private Dictionary<long, Dictionary<long, List<long>>> recloserOutageMap;
        private OutageMessageMapper outageMessageMapper;
        private IOutageTopologyModel topologyModel;
        private Dictionary<long, long> CommandedElements;
        private Dictionary<long, long> OptimumIsolationPoints;
		private IOutageModelReadAccessContract outageModelReadAccessClient;
        private IHistoryDBManagerContract historyDBManagerClient;
		private ICloudLogger logger;

		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}
		public ReportOutageService(UnitOfWork dbContext)
		{
			this.dbContext = dbContext;
			this.recloserOutageMap = new Dictionary<long, Dictionary<long, List<long>>>();
			this.outageModelReadAccessClient = OutageModelReadAccessClient.CreateClient();
            this.historyDBManagerClient = HistoryDBManagerClient.CreateClient();
            this.outageMessageMapper = new OutageMessageMapper();

          
		}

        public Task<bool> IsAlive()
        {
            return Task.Run(() => { return true; });
        }
        public async Task InitAwaitableFields()
        {
            this.topologyModel = await outageModelReadAccessClient.GetTopologyModel();
            this.CommandedElements = await outageModelReadAccessClient.GetCommandedElements();
            this.OptimumIsolationPoints = await outageModelReadAccessClient.GetOptimumIsolatioPoints();
            this.outageLifecycleHelper = new OutageLifecycleHelper(this.dbContext, this.topologyModel);
        }
        public async Task<bool> ReportPotentialOutage(long gid, CommandOriginType commandOriginType)
        {
            await InitAwaitableFields();
            bool success = false;
            List<long> affectedConsumersIds = new List<long>();
            affectedConsumersIds = outageLifecycleHelper.GetAffectedConsumers(gid);
            if (commandOriginType != CommandOriginType.USER_COMMAND && commandOriginType != CommandOriginType.ISOLATING_ALGORITHM_COMMAND)
            {
                Logger.LogDebug($"Reporting outage for gid: 0x{gid:X16}");

                if (this.CommandedElements.ContainsKey(gid) || this.OptimumIsolationPoints.ContainsKey(gid))
                {
                    await historyDBManagerClient.OnSwitchOpened(gid, null);
                    await historyDBManagerClient.OnConsumerBlackedOut(affectedConsumersIds, null);
                    return false;
                }

                if (affectedConsumersIds.Count == 0)
                {
                    bool isSwitchInvoked = false;
                    if (recloserOutageMap.TryGetValue(gid, out Dictionary<long, List<long>> outageAffectedPair))
                    {
                        foreach (var pair in outageAffectedPair)
                        {
                            await historyDBManagerClient.OnConsumerBlackedOut(pair.Value, pair.Key);
                            await historyDBManagerClient.OnSwitchOpened(gid, pair.Key);
                            isSwitchInvoked = true;
                        }
                    }

                    if (!isSwitchInvoked)
                    {
                        await historyDBManagerClient.OnSwitchOpened(gid, null);
                    }

                    Logger.LogInformation("There is no affected consumers => outage report is not valid.");
                    return false;
                }

                OutageEntity activeOutageDbEntity = null;
                if(this.dbContext.OutageRepository.Find(o => o.OutageElementGid == gid && o.OutageState != OutageState.ARCHIVED).FirstOrDefault() != null)
                {
                    Logger.LogWarning($"Malfunction on element with gid: 0x{gid:x16} has already been reported.");
                    return false;
                }

                List<Consumer> consumerDbEntities = outageLifecycleHelper.GetAffectedConsumersFromDatabase(affectedConsumersIds);
                if(consumerDbEntities.Count != affectedConsumersIds.Count)
                {
                    Logger.LogWarning("Some of affected consumers are not present in database.");
                    return false;
                }

                long recloserId;
                try
                {
                    recloserId = this.outageLifecycleHelper.GetRecloserForHeadBreaker(gid);
                }
                catch (Exception)
                {

                    return false;
                }

                List<Equipment> defaultIsolationPoints = await outageLifecycleHelper.GetEquipmentEntity(new List<long> { gid, recloserId });

                OutageEntity createdActiveOutage = new OutageEntity
                {
                    AffectedConsumers = consumerDbEntities,
                    OutageState = OutageState.CREATED,
                    ReportTime = DateTime.UtcNow,
                    DefaultIsolationPoints = defaultIsolationPoints,
                };

                activeOutageDbEntity = dbContext.OutageRepository.Add(createdActiveOutage);

                try
                {
                    dbContext.Complete();
                    Logger.LogDebug($"Outage on element with gid: 0x{activeOutageDbEntity.OutageElementGid:x16} is successfully stored in database.");
                    success = true;

                    if (recloserOutageMap.TryGetValue(recloserId, out Dictionary<long, List<long>> outageAffectedPair))
                    {
                        if (outageAffectedPair.TryGetValue(createdActiveOutage.OutageId, out List<long> affected))
                        {
                            affected = new List<long>(affectedConsumersIds);
                        }
                        else
                        {
                            outageAffectedPair.Add(createdActiveOutage.OutageId, affectedConsumersIds);
                        }
                    }
                    else
                    {
                        Dictionary<long, List<long>> dict = new Dictionary<long, List<long>>()
                        {
                            { createdActiveOutage.OutageId, affectedConsumersIds }
                        };

                        recloserOutageMap.Add(recloserId, dict);
                    }

                    await historyDBManagerClient.OnSwitchOpened(gid, createdActiveOutage.OutageId);
                    await historyDBManagerClient.OnConsumerBlackedOut(affectedConsumersIds, createdActiveOutage.OutageId);
                }
                catch (Exception e)
                {
                    string message = "OutageModel::ReportPotentialOutage method => exception on Complete()";
                    Logger.LogError(message, e);
                    Console.WriteLine($"{message}, Message: {e.Message}, Inner Message: {e.InnerException.Message})");

                    //TODO: da li je dobar handle?
                    dbContext.Dispose();
                    dbContext = new UnitOfWork();
                    success = false;
                }

                if (success && activeOutageDbEntity != null)
                {
                    try
                    {
                        success = await outageLifecycleHelper.PublishOutage(Topic.ACTIVE_OUTAGE, outageMessageMapper.MapOutageEntity(activeOutageDbEntity));

                        if (success)
                        {
                            Logger.LogInformation($"Outage on element with gid: 0x{activeOutageDbEntity.OutageElementGid:x16} is successfully published");
                        }
                    }
                    catch (Exception e) //TODO: Exception over proxy or enum...
                    {
                        Logger.LogError("OutageModel::ReportPotentialOutage => exception on PublishActiveOutage()", e);
                        success = false;
                    }
                }
            }
            return success;
		}
    }
}

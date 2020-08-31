using Common.OMS;
using Common.OmsContracts.DataContracts.OutageDatabaseModel;
using Common.OmsContracts.ModelAccess;
using Common.OmsContracts.OutageLifecycle;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.WcfClient.OMS.ModelAccess;
using OMS.OutageLifecycleImplementation.OutageLCHelper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.OutageLifecycleImplementation
{
    public class ResolveOutageService : IResolveOutageContract
	{

		private ICloudLogger logger;

		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}
		private OutageLifecycleHelper lifecycleHelper;
        private OutageMessageMapper outageMessageMapper;
        private IOutageAccessContract outageModelAccessClient;
        public ResolveOutageService()
        {
			this.lifecycleHelper = new OutageLifecycleHelper(null);
            this.outageMessageMapper = new OutageMessageMapper();
            this.outageModelAccessClient = OutageModelAccessClient.CreateClient();
        }
        public Task<bool> IsAlive()
        {
            return Task.Run(() => { return true; });
        }
        public async Task<bool> ResolveOutage(long outageId)
		{
            Logger.LogDebug("ResolveOutage method started.");
            OutageEntity activeOutageDbEntity = null;

            try
            {
                activeOutageDbEntity = await outageModelAccessClient.GetOutage(outageId);
            }
            catch (Exception e)
            {
                string message = "OutageModel::SendRepairCrew => exception in UnitOfWork.ActiveOutageRepository.Get()";
                Logger.LogError(message, e);
                throw e;
            }

            if (activeOutageDbEntity == null)
            {
                Logger.LogError($"Outage with id 0x{outageId:X16} is not found in database.");
                return false;
            }

            if (activeOutageDbEntity.OutageState != OutageState.REPAIRED)
            {
                Logger.LogError($"Outage with id 0x{outageId:X16} is in state {activeOutageDbEntity.OutageState}, and thus repair crew can not be sent. (Expected state: {OutageState.REPAIRED})");
                return false;
            }

            if (!activeOutageDbEntity.IsResolveConditionValidated)
            {
                //MODO: mozda i ovde odraditi proveru uslova? ako je IsResolveConditionValidated == false, a po svemu sudeci ne bi smeo biti na ovom mestu onda ponovo pokrenuti rutinu provere za setovanje tog "isValid" - egde case, ako se cackaju entiteti u bazi...
                Logger.LogWarning("ResolveOutage => resolve conditions not validated.");
                return false;
            }

            OutageEntity createdArchivedOutage = new OutageEntity()
            {
                OutageId = activeOutageDbEntity.OutageId,
                OutageState = OutageState.ARCHIVED,
                OutageElementGid = activeOutageDbEntity.OutageElementGid,
                IsResolveConditionValidated = activeOutageDbEntity.IsResolveConditionValidated,
                ReportTime = activeOutageDbEntity.ReportTime,
                IsolatedTime = activeOutageDbEntity.IsolatedTime,
                RepairedTime = activeOutageDbEntity.RepairedTime,
                ArchivedTime = DateTime.UtcNow,
                DefaultIsolationPoints = new List<Equipment>(activeOutageDbEntity.DefaultIsolationPoints),
                OptimumIsolationPoints = new List<Equipment>(activeOutageDbEntity.OptimumIsolationPoints),
                AffectedConsumers = new List<Consumer>(activeOutageDbEntity.AffectedConsumers),
            };

            bool success;
            await this.outageModelAccessClient.RemoveOutage(activeOutageDbEntity);
            OutageEntity archivedOutageDbEntity = null;

            try
            {
                archivedOutageDbEntity = await outageModelAccessClient.AddOutage(createdArchivedOutage);
                Logger.LogDebug($"ArchivedOutage on element with gid: 0x{archivedOutageDbEntity.OutageElementGid:x16} is successfully stored in database.");
                success = true;
            }
            catch (Exception e)
            {
                string message = "OutageModel::ResolveOutage method => exception on Complete()";
                Logger.LogError(message, e);
                Console.WriteLine($"{message}, Message: {e.Message})");

                //TODO: da li je dobar handle?
                //dbContext.Dispose();
                //dbContext = new UnitOfWork();
                success = false;
            }

            if (success && archivedOutageDbEntity != null) //TODO: ne svidja mi se ova konstrukcija...
            {
                try
                {
                    success = await lifecycleHelper.PublishOutage(Topic.ARCHIVED_OUTAGE, outageMessageMapper.MapOutageEntity(archivedOutageDbEntity));

                    if (success)
                    {
                        Logger.LogInformation($"ArchivedOutage on element with gid: 0x{archivedOutageDbEntity.OutageElementGid:x16} is successfully published");
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError("OutageModel::ResolveOutage => exception on PublishActiveOutage()", e);
                    success = false;
                }
            }

            return success;
        }
	}
}

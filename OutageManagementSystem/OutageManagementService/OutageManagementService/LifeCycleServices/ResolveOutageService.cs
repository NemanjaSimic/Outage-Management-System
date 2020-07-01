using OMSCommon.Mappers;
using OMSCommon.OutageDatabaseModel;
using Outage.Common;
using OutageDatabase.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutageManagementService.LifeCycleServices
{
    public class ResolveOutageService
    {
        private OutageModel outageModel;
        private ILogger logger;

        private ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        private UnitOfWork dbContext;
        private OutageMessageMapper outageMessageMapper;

        public ResolveOutageService(OutageModel outageModel)
        {
            this.outageModel = outageModel;
            dbContext = outageModel.dbContext;
            outageMessageMapper = new OutageMessageMapper();
        }

        public bool ResolveOutage(long outageId)
        {
            OutageEntity activeOutageDbEntity = null;

            try
            {
                activeOutageDbEntity = dbContext.OutageRepository.Get(outageId);
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
                //TODO: mozda i ovde odraditi proveru uslova?
                Logger.LogWarn("ResolveOutage => resolve conditions not validated.");
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
                DefaultIsolationPoints =  new List<Equipment>(activeOutageDbEntity.DefaultIsolationPoints),
                OptimumIsolationPoints = new List<Equipment>(activeOutageDbEntity.OptimumIsolationPoints),
                AffectedConsumers = new List<Consumer>(activeOutageDbEntity.AffectedConsumers),
            };

            bool success;
            dbContext.OutageRepository.Remove(activeOutageDbEntity);
            OutageEntity archivedOutageDbEntity = dbContext.OutageRepository.Add(createdArchivedOutage);

            try
            {
                dbContext.Complete();
                Logger.LogDebug($"ArchivedOutage on element with gid: 0x{archivedOutageDbEntity.OutageElementGid:x16} is successfully stored in database.");
                success = true;
            }
            catch (Exception e)
            {
                string message = "OutageModel::ResolveOutage method => exception on Complete()";
                Logger.LogError(message, e);
                Console.WriteLine($"{message}, Message: {e.Message})");

                //TODO: da li je dobar handle?
                dbContext.Dispose();
                dbContext = new UnitOfWork();
                success = false;
            }

            if (success && archivedOutageDbEntity != null) //TODO: ne svidja mi se ova konstrukcija...
            {
                try
                {
                    success = outageModel.PublishOutage(Topic.ARCHIVED_OUTAGE, outageMessageMapper.MapOutageEntity(archivedOutageDbEntity));

                    if (success)
                    {
                        Logger.LogInfo($"ArchivedOutage on element with gid: 0x{archivedOutageDbEntity.OutageElementGid:x16} is successfully published");
                    }
                }
                catch (Exception e) //TODO: Exception over proxy or enum...
                {
                    Logger.LogError("OutageModel::ResolveOutage => exception on PublishActiveOutage()", e);
                    success = false;
                }
            }

            return success;
        }
    }
}

using OMSCommon.Mappers;
using OMSCommon.OutageDatabaseModel;
using Outage.Common;
using Outage.Common.OutageService.Interface;
using OutageDatabase.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutageManagementService.LifeCycleServices
{
    public class ValidateResolveConditionsService
    {
        private OutageModel outageModel;
        private ILogger logger;

        private ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        private UnitOfWork dbContext;
        private OutageMessageMapper outageMessageMapper;

        public ValidateResolveConditionsService(OutageModel outageModel)
        {
            this.outageModel = outageModel;
            dbContext = outageModel.dbContext;
            outageMessageMapper = new OutageMessageMapper();
        }


        public bool ValidateResolveConditions(long outageId)
        {
            OutageEntity outageDbEntity = null;

            try
            {
                outageDbEntity = dbContext.OutageRepository.Get(outageId);
            }
            catch (Exception e)
            {
                string message = "OutageModel::SendRepairCrew => exception in UnitOfWork.ActiveOutageRepository.Get()";
                Logger.LogError(message, e);
                throw e;
            }

            if (outageDbEntity == null)
            {
                Logger.LogError($"Outage with id 0x{outageId:X16} is not found in database.");
                return false;
            }

            if (outageDbEntity.OutageState != OutageState.REPAIRED)
            {
                Logger.LogError($"Outage with id 0x{outageId:X16} is in state {outageDbEntity.OutageState}, and thus repair crew can not be sent. (Expected state: {OutageState.REPAIRED})");
                return false;
            }

            List<Equipment> isolationPoints = new List<Equipment>();
            isolationPoints.AddRange(outageDbEntity.DefaultIsolationPoints);
            isolationPoints.AddRange(outageDbEntity.OptimumIsolationPoints);

            bool resolveCondition = true;

            foreach (Equipment isolationPoint in isolationPoints)
            {
                if (outageModel.TopologyModel.GetElementByGid(isolationPoint.EquipmentId, out IOutageTopologyElement element))
                {
                    if (element.NoReclosing != element.IsActive)
                    {
                        resolveCondition = false;
                        break;
                    }
                }
            }

            outageDbEntity.IsResolveConditionValidated = resolveCondition;

            dbContext.OutageRepository.Update(outageDbEntity);

            try
            {
                dbContext.Complete();
                outageModel.PublishOutage(Topic.ACTIVE_OUTAGE, outageMessageMapper.MapOutageEntity(outageDbEntity));
            }
            catch (Exception e)
            {
                string message = "OutageModel::ValidateResolveConditions => exception in Complete method.";
                Logger.LogError(message, e);
            }

            return true;
        }
    }
}

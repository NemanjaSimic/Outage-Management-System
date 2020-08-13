using Common.OMS;
using Common.OMS.OutageDatabaseModel;
using Common.OmsContracts.ModelProvider;
using Common.OmsContracts.OutageLifecycle;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.PubSubContracts.Interfaces;
using OMS.Common.WcfClient.OMS;
using OMS.OutageLifecycleServiceImplementation.OutageLCHelper;
using OutageDatabase.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.OutageLifecycleServiceImplementation
{
    public class ValidateResolveConditionsService : IValidateResolveConditionsContract
	{
        private IOutageTopologyModel outageModel;
        private ICloudLogger logger;

        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        private UnitOfWork dbContext;
        private OutageMessageMapper outageMessageMapper;
        private OutageLifecycleHelper outageLifecycleHelper;
        private IOutageModelReadAccessContract outageModelReadAccessClient;
        public ValidateResolveConditionsService(UnitOfWork dbContext)
        {
            this.dbContext = dbContext;
            this.outageMessageMapper = new OutageMessageMapper();
            this.outageModelReadAccessClient = OutageModelReadAccessClient.CreateClient();
        }

        public async Task InitAwaitableFields()
        {
            this.outageModel = await outageModelReadAccessClient.GetTopologyModel();
            this.outageLifecycleHelper = new OutageLifecycleHelper(this.dbContext, this.outageModel);
        }
        public Task<bool> IsAlive()
        {
            return Task.Run(() => { return true; });
        }
        public async Task<bool> ValidateResolveConditions(long outageId)
		{
            await InitAwaitableFields();
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
                if (outageModel.GetElementByGid(isolationPoint.EquipmentId, out IOutageTopologyElement element))
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
                await outageLifecycleHelper.PublishOutage(Topic.ACTIVE_OUTAGE, outageMessageMapper.MapOutageEntity(outageDbEntity));
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

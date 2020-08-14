using Common.CE;
using Common.OMS;
using Common.OMS.OutageDatabaseModel;
using Common.OmsContracts.ModelAccess;
using Common.OmsContracts.ModelProvider;
using Common.OmsContracts.OutageLifecycle;
using Common.OmsContracts.OutageSimulator;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Names;
using OMS.Common.PubSub;
using OMS.Common.WcfClient.OMS;
using OMS.Common.WcfClient.OMS.ModelAccess;
using OMS.OutageLifecycleServiceImplementation.OutageLCHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace OMS.OutageLifecycleServiceImplementation
{
    public class SendRepairCrewService : ISendRepairCrewContract
    {
        private IOutageTopologyModel outageModel;
        private ICloudLogger logger;

        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        private OutageMessageMapper outageMessageMapper;
        private OutageLifecycleHelper outageLifecycleHelper;

		#region MyRegion
		private IOutageModelReadAccessContract outageModelReadAccessClient;
        private IOutageAccessContract outageModelAccessClient;
		#endregion

        public SendRepairCrewService()
        {
            this.outageMessageMapper = new OutageMessageMapper();
            this.outageModelReadAccessClient = OutageModelReadAccessClient.CreateClient();
            this.outageModelAccessClient = OutageModelAccessClient.CreateClient();
            

        }
        public async Task InitAwaitableFields()
        {
            this.outageModel = await outageModelReadAccessClient.GetTopologyModel();
            this.outageLifecycleHelper = new OutageLifecycleHelper(this.outageModel);
        }
        public async Task<bool> SendRepairCrew(long outageId)
        {
            Logger.LogDebug("SendRepairCrew method started.");
            await InitAwaitableFields();
            OutageEntity outageDbEntity = null;

            try
            {
                outageDbEntity = await outageModelAccessClient.GetOutage(outageId);
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

            if (outageDbEntity.OutageState != OutageState.ISOLATED)
            {
                Logger.LogError($"Outage with id 0x{outageId:X16} is in state {outageDbEntity.OutageState}, and thus repair crew can not be sent. (Expected state: {OutageState.ISOLATED})");
                return false;
            }

            await Task.Delay(10000);

            IOutageSimulatorContract outageSimulatorClient = OutageSimulatorClient.CreateClient();
            if (await outageSimulatorClient.StopOutageSimulation(outageDbEntity.OutageElementGid))
            {
                outageDbEntity.OutageState = OutageState.REPAIRED;
                outageDbEntity.RepairedTime = DateTime.UtcNow;
                await outageModelAccessClient.UpdateOutage(outageDbEntity);

                try
                {
                    await outageLifecycleHelper.PublishOutage(Topic.ACTIVE_OUTAGE, outageMessageMapper.MapOutageEntity(outageDbEntity));
                }
                catch (Exception e)
                {
                    string message = "OutageModel::SendRepairCrew => exception in Complete method.";
                    Logger.LogError(message, e);
                }
            }
            else
            {
                string message = "OutageModel::SendRepairCrew => ResolvedOutage() not finished with SUCCESS";
                Logger.LogError(message);
            }

            return true;
        }
    }
}

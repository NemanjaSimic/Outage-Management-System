using Common.CE;
using Common.OMS;
using Common.OMS.OutageDatabaseModel;
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
		private OutageModelReadAccessClient outageModelReadAccessClient;
        private OutageModelAccessClient outageModelAccessClient;
		#endregion

        private ChannelFactory<IOutageSimulatorContract> channelFactory = new ChannelFactory<IOutageSimulatorContract>(EndpointNames.OmsOutageSimulatorEndpoint);
        private IOutageSimulatorContract proxy;
        public SendRepairCrewService()
        {
            this.outageMessageMapper = new OutageMessageMapper();
            this.outageModelReadAccessClient = OutageModelReadAccessClient.CreateClient();
            this.outageModelAccessClient = OutageModelAccessClient.CreateClient();
            this.proxy = channelFactory.CreateChannel();

        }
        public async Task InitAwaitableFields()
        {
            this.outageModel = await outageModelReadAccessClient.GetTopologyModel();
            this.outageLifecycleHelper = new OutageLifecycleHelper(this.outageModel);
        }
        public async Task<bool> SendRepairCrew(long outageId)
		{
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
       
            Task task = Task.Run(async () =>
            {
                Task.Delay(10000).Wait();

                
                    if (proxy == null)
                    {
                        string message = "OutageModel::SendRepairCrew => OutageSimulatorServiceProxy is null";
                        Logger.LogError(message);
                        throw new NullReferenceException(message);
                    }

                if (proxy.StopOutageSimulation(outageDbEntity.OutageElementGid))
                {
                    outageDbEntity.OutageState = OutageState.REPAIRED;
                    outageDbEntity.RepairedTime = DateTime.UtcNow;
                    

                    try
                    {
                        await outageModelAccessClient.UpdateOutage(outageDbEntity);
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
                
            });

            return true;
        }
	}
}

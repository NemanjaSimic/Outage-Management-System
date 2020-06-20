using OMSCommon.Mappers;
using OMSCommon.OutageDatabaseModel;
using Outage.Common;
using Outage.Common.ServiceContracts.OMS;
using Outage.Common.ServiceProxies;
using Outage.Common.ServiceProxies.Outage;
using OutageDatabase.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutageManagementService.LifeCycleServices
{
    public class SendRepairCrewService
    {
        private OutageModel outageModel;
        private ILogger logger;

        private ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        private UnitOfWork dbContext;
        private ProxyFactory proxyFactory;
        private OutageMessageMapper outageMessageMapper;


        public SendRepairCrewService(OutageModel outageModel)
        {
            this.outageModel = outageModel;
            dbContext = outageModel.dbContext;
            proxyFactory = new ProxyFactory();
            outageMessageMapper = new OutageMessageMapper();
        }

        public bool SendRepairCrew(long outageId)
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

            if (outageDbEntity.OutageState != OutageState.ISOLATED)
            {
                Logger.LogError($"Outage with id 0x{outageId:X16} is in state {outageDbEntity.OutageState}, and thus repair crew can not be sent. (Expected state: {OutageState.ISOLATED})");
                return false;
            }

            Task task = Task.Run(() =>
            {
                Task.Delay(10000).Wait();

                using (OutageSimulatorServiceProxy proxy = proxyFactory.CreateProxy<OutageSimulatorServiceProxy, IOutageSimulatorContract>(EndpointNames.OutageSimulatorServiceEndpoint))
                {
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
                        dbContext.OutageRepository.Update(outageDbEntity);

                        try
                        {
                            dbContext.Complete();
                            outageModel.PublishOutage(Topic.ACTIVE_OUTAGE, outageMessageMapper.MapOutageEntity(outageDbEntity));
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
                }
            });

            return true;
        }
    }
}

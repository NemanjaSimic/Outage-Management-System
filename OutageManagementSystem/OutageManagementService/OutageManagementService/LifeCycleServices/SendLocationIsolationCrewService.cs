using OMSCommon.Mappers;
using OMSCommon.OutageDatabaseModel;
using Outage.Common;
using Outage.Common.OutageService.Interface;
using Outage.Common.ServiceContracts.OMS;
using Outage.Common.ServiceProxies;
using Outage.Common.ServiceProxies.Outage;
using OutageDatabase.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OutageManagementService.LifeCycleServices
{
    public class SendLocationIsolationCrewService
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
        private bool result = false;
        public SendLocationIsolationCrewService(OutageModel outageModel)
        {
            this.outageModel = outageModel;
            dbContext = outageModel.dbContext;
            proxyFactory = new ProxyFactory();
            outageMessageMapper = new OutageMessageMapper();
        }
        public bool SendLocationIsolationCrew(long outageId)
        {
            OutageEntity outageEntity = null;
            List<OutageEntity> activeOutages = null;
            long reportedGid = 0;
            bool result = false;
            try
            {
                outageEntity = dbContext.OutageRepository.Get(outageId);
            }
            catch (Exception ex)
            {

                Logger.LogError($"OutageModel::SendLocationIsolationCrew => exception in UnitOfWork.OutageRepository.Get()", ex);
            }

            try
            {
                activeOutages = dbContext.OutageRepository.GetAllActive().ToList();
            }
            catch (Exception ex)
            {

                Logger.LogError("OutageModel::SendLocationIsolationCrew => excpetion in UnitOfWork.OutageRepository.GetAllActive()", ex);
                return false;
            }
            if (outageEntity == null)
            {
                Logger.LogError($"Outage with id 0x{outageId:X16} is not found in database.");
                return false;
            }

            reportedGid = outageEntity.DefaultIsolationPoints.First().EquipmentId;

            Task algorithm =  Task.Run(() => StartLocationAndIsolationAlgorithm(outageEntity)).ContinueWith(task =>
            {
                result = task.Result;
                if (task.IsCompleted)
                {
                    dbContext.Complete();
                }
            }, TaskContinuationOptions.OnlyOnRanToCompletion).ContinueWith(task =>
            {
                outageModel.PublishOutage(Topic.ACTIVE_OUTAGE, outageMessageMapper.MapOutageEntity(outageEntity));
      
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
            algorithm.Wait();
          
            return result;
        }
        public bool StartLocationAndIsolationAlgorithm(OutageEntity outageEntity)
        {
       
            IOutageTopologyElement topologyElement = null;
         
            long reportedGid = outageEntity.DefaultIsolationPoints.First().EquipmentId;
            long outageElementId = -1;
            long UpBreaker;

            Task.Delay(5000).Wait();
            using (OutageSimulatorServiceProxy proxy = proxyFactory.CreateProxy<OutageSimulatorServiceProxy, IOutageSimulatorContract>(EndpointNames.OutageSimulatorServiceEndpoint))
            {

                if (proxy == null)
                {
                    string message = "OutageModel::SendLocationIsolationCrew => OutageSimulatorProxy is null";
                    Logger.LogError(message);
                    throw new NullReferenceException(message);
                }
                //Da li je prijaveljen element OutageElement
                if (proxy.IsOutageElement(reportedGid))
                {
                    outageElementId = reportedGid;
                }
                else
                {
                    //Da li je mozda na ACL-novima ispod prijavljenog elementa
                    if (outageModel.TopologyModel.GetElementByGid(reportedGid, out topologyElement))
                    {
                        try
                        {
                            for (int i = 0; i < topologyElement.SecondEnd.Count; i++)
                            {
                                if (proxy.IsOutageElement(topologyElement.SecondEnd[i]))
                                {
                                    if (outageElementId == -1)
                                    {
                                        outageElementId = topologyElement.SecondEnd[i];
                                        outageEntity.OutageElementGid = outageElementId;
                                    }
                                    else
                                    {
                                        OutageEntity entity = new OutageEntity() { OutageElementGid = topologyElement.SecondEnd[i], ReportTime = DateTime.UtcNow };
                                        dbContext.OutageRepository.Add(entity);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError("OutageModel::SendLocationIsolationCrew => failed with error", ex);
                            throw;
                        }
                    }
                }
                //Tragamo za ACL-om gore ka source-u
                while (outageElementId == -1 && !topologyElement.IsRemote && topologyElement.DmsType != "ENERGYSOURCE")
                {
                    if (proxy.IsOutageElement(topologyElement.Id))
                    {
                        outageElementId = topologyElement.Id;
                        outageEntity.OutageElementGid = outageElementId;
                    }
                    outageModel.TopologyModel.GetElementByGid(topologyElement.FirstEnd, out topologyElement);
                }
                if(outageElementId == -1)
                {
                    outageEntity.OutageState = OutageState.REMOVED;
                    dbContext.OutageRepository.Remove(outageEntity);
                    Logger.LogError("End of feeder no outage detected.");
                    return false;
                }
                outageModel.TopologyModel.GetElementByGidFirstEnd(outageEntity.OutageElementGid, out topologyElement);
                while (topologyElement.DmsType != "BREAKER")
                {
                    outageModel.TopologyModel.GetElementByGidFirstEnd(topologyElement.Id, out topologyElement);
                }
                UpBreaker = topologyElement.Id;
                long nextBreaker = outageModel.GetNextBreaker(outageEntity.OutageElementGid);

                if (!outageModel.TopologyModel.OutageTopology.ContainsKey(nextBreaker))
                {
                    string message = $"Breaker (next breaker) with id: 0x{nextBreaker:X16} is not in topology";
                    Logger.LogError(message);
                    throw new Exception(message);
                }

                long outageElement = outageModel.TopologyModel.OutageTopology[nextBreaker].FirstEnd;

                if (!outageModel.TopologyModel.OutageTopology[UpBreaker].SecondEnd.Contains(outageElement))
                {
                    string message = $"Outage element with gid: 0x{outageElement:X16} is not on a second end of current breaker id";
                    Logger.LogError(message);
                    throw new Exception(message);
                }
                outageEntity.OptimumIsolationPoints = outageModel.GetEquipmentEntity(new List<long>() { UpBreaker, nextBreaker });
                outageEntity.IsolatedTime = DateTime.UtcNow;
                outageEntity.OutageState = OutageState.ISOLATED;

                dbContext.OutageRepository.Update(outageEntity);
             

            }
       
            return true;
        }

    }
}

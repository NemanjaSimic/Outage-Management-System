using Common.CE;
using Common.CeContracts;
using Common.OMS;
using Common.OMS.OutageDatabaseModel;
using Common.OmsContracts.ModelProvider;
using Common.OmsContracts.OutageLifecycle;
using Common.OmsContracts.OutageSimulator;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.Names;
using OMS.Common.PubSub;
using OMS.Common.WcfClient.CE;
using OMS.Common.WcfClient.OMS;
using OMS.OutageLifecycleServiceImplementation.OutageLCHelper;
using OutageDatabase.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace OMS.OutageLifecycleServiceImplementation
{
    public class SendLocationIsolationCrewService : ISendLocationIsolationCrewContract
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
        private IOutageModelUpdateAccessContract outageModelUpdateAccessClient;
        private IMeasurementMapContract measurementMapServiceClient;
        private ISwitchStatusCommandingContract switchStatusCommandingClient;
        private Dictionary<long, long> CommandedElements;
        public SendLocationIsolationCrewService(UnitOfWork dbContext)
        {
            this.dbContext = dbContext;
            this.outageMessageMapper = new OutageMessageMapper();
            this.outageModelReadAccessClient = OutageModelReadAccessClient.CreateClient();
            this.outageModelUpdateAccessClient = OutageModelUpdateAccessClient.CreateClient();
            this.measurementMapServiceClient =  MeasurementMapServiceClient.CreateClient();
            this.switchStatusCommandingClient = SwitchStatusCommandingClient.CreateClient();
            this.CommandedElements = new Dictionary<long, long>();
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
        public async Task<bool> SendLocationIsolationCrew(long outageId)
        {
            await InitAwaitableFields();
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

            Task algorithm = Task.Run(() => StartLocationAndIsolationAlgorithm(outageEntity)).ContinueWith(task =>
            {
                result = task.Result;
                if (task.IsCompleted)
                {
                    dbContext.Complete();
                }
            }, TaskContinuationOptions.OnlyOnRanToCompletion).ContinueWith(async task =>
            {
                await outageLifecycleHelper.PublishOutage(Topic.ACTIVE_OUTAGE, outageMessageMapper.MapOutageEntity(outageEntity));

            }, TaskContinuationOptions.OnlyOnRanToCompletion);
            algorithm.Wait();

            return result;
        }
        public async Task<bool> StartLocationAndIsolationAlgorithm(OutageEntity outageEntity)
        {

            IOutageTopologyElement topologyElement = null;

            long reportedGid = outageEntity.DefaultIsolationPoints.First().EquipmentId;
            long outageElementId = -1;
            long UpBreaker;

            Task.Delay(5000).Wait();


            IOutageSimulatorContract outageSimulatorClient = OutageSimulatorClient.CreateClient();
            //Da li je prijaveljen element OutageElement
            if (await outageSimulatorClient.IsOutageElement(reportedGid))
            {
                outageElementId = reportedGid;
            }
            else
            {
                //Da li je mozda na ACL-novima ispod prijavljenog elementa
                if (outageModel.GetElementByGid(reportedGid, out topologyElement))
                {
                    try
                    {
                        for (int i = 0; i < topologyElement.SecondEnd.Count; i++)
                        {
                            if (await outageSimulatorClient.IsOutageElement(topologyElement.SecondEnd[i]))
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
                if (await outageSimulatorClient.IsOutageElement(topologyElement.Id))
                {
                    outageElementId = topologyElement.Id;
                    outageEntity.OutageElementGid = outageElementId;
                }
                outageModel.GetElementByGid(topologyElement.FirstEnd, out topologyElement);
            }
            if (outageElementId == -1)
            {
                outageEntity.OutageState = OutageState.REMOVED;
                dbContext.OutageRepository.Remove(outageEntity);
                Logger.LogError("End of feeder no outage detected.");
                return false;
            }
            outageModel.GetElementByGidFirstEnd(outageEntity.OutageElementGid, out topologyElement);
            while (topologyElement.DmsType != "BREAKER")
            {
                outageModel.GetElementByGidFirstEnd(topologyElement.Id, out topologyElement);
            }
            UpBreaker = topologyElement.Id;
            long nextBreaker = outageLifecycleHelper.GetNextBreaker(outageEntity.OutageElementGid);

            if (!outageModel.OutageTopology.ContainsKey(nextBreaker))
            {
                string message = $"Breaker (next breaker) with id: 0x{nextBreaker:X16} is not in topology";
                Logger.LogError(message);
                throw new Exception(message);
            }
            long outageElement = outageModel.OutageTopology[nextBreaker].FirstEnd;

            if (!outageModel.OutageTopology[UpBreaker].SecondEnd.Contains(outageElement))
            {
                string message = $"Outage element with gid: 0x{outageElement:X16} is not on a second end of current breaker id";
                Logger.LogError(message);
                throw new Exception(message);
            }
            outageEntity.OptimumIsolationPoints = await outageLifecycleHelper.GetEquipmentEntity(new List<long>() { UpBreaker, nextBreaker });
            outageEntity.IsolatedTime = DateTime.UtcNow;
            outageEntity.OutageState = OutageState.ISOLATED;

            dbContext.OutageRepository.Update(outageEntity);
            SendSCADACommand(UpBreaker, DiscreteCommandingType.OPEN);
            SendSCADACommand(nextBreaker, DiscreteCommandingType.OPEN);



            return true;
        }
        //TO DO use CE client
        private async void SendSCADACommand(long currentBreakerId, DiscreteCommandingType discreteCommandingType)
        {
            long measurement = -1;

            List<long> measuremnts = new List<long>();
            try
            {
                measuremnts = measurementMapServiceClient.GetMeasurementsOfElement(currentBreakerId).Result;

            }
            catch (Exception e)
            {
                //Logger.LogError("Error on GetMeasurementsForElement() method", e);
                throw e;
            }

            if (measuremnts.Count > 0)
            {
                measurement = measuremnts[0];
            }

            CommandedElements = await outageModelReadAccessClient.GetCommandedElements();

            if (measurement != -1)
            {
                if (discreteCommandingType == DiscreteCommandingType.OPEN && !CommandedElements.ContainsKey(currentBreakerId))
                {
                    //TODO: add at list
                    await this.outageModelUpdateAccessClient.UpdateCommandedElements(currentBreakerId,ModelUpdateOperationType.INSERT);
                }



                try
                {
                    await switchStatusCommandingClient.SendOpenCommand(measurement);
                }
                catch (Exception e)
                {
                    if (discreteCommandingType == DiscreteCommandingType.OPEN && CommandedElements.ContainsKey(currentBreakerId))
                    {
                        await this.outageModelUpdateAccessClient.UpdateCommandedElements(currentBreakerId, ModelUpdateOperationType.DELETE);
                    }
                    throw e;
                }
                
            }
         
        }
    }
}

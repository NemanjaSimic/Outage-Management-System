using Common.CeContracts;
using Common.OMS;
using Common.OmsContracts.DataContracts.OutageDatabaseModel;
using Common.OmsContracts.ModelAccess;
using Common.OmsContracts.ModelProvider;
using Common.OmsContracts.OutageLifecycle;
using Common.OmsContracts.OutageSimulator;
using Common.PubSubContracts.DataContracts.CE;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.WcfClient.CE;
using OMS.Common.WcfClient.OMS;
using OMS.Common.WcfClient.OMS.ModelAccess;
using OMS.OutageLifecycleImplementation.OutageLCHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OMS.OutageLifecycleImplementation
{
    public class SendLocationIsolationCrewService : ISendLocationIsolationCrewContract
    {
        private OutageTopologyModel outageModel;
        private ICloudLogger logger;

        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        private OutageMessageMapper outageMessageMapper;
        private OutageLifecycleHelper outageLifecycleHelper;

        #region Clients
        private IOutageModelReadAccessContract outageModelReadAccessClient;
        private IOutageModelUpdateAccessContract outageModelUpdateAccessClient;
        private IMeasurementMapContract measurementMapServiceClient;
        private ISwitchStatusCommandingContract switchStatusCommandingClient;
        private IOutageAccessContract outageModelAccessClient;
        #endregion

        private Dictionary<long, long> CommandedElements;
        public SendLocationIsolationCrewService()
        {
            this.outageMessageMapper = new OutageMessageMapper();

            this.outageModelReadAccessClient = OutageModelReadAccessClient.CreateClient();
            this.outageModelUpdateAccessClient = OutageModelUpdateAccessClient.CreateClient();
            this.outageModelAccessClient = OutageModelAccessClient.CreateClient();
            this.measurementMapServiceClient = MeasurementMapClient.CreateClient();
            this.switchStatusCommandingClient = SwitchStatusCommandingClient.CreateClient();

            this.CommandedElements = new Dictionary<long, long>();
        }
        public async Task InitAwaitableFields()
        {
            this.outageModel = await outageModelReadAccessClient.GetTopologyModel();
            this.outageLifecycleHelper = new OutageLifecycleHelper(this.outageModel);
        }
        public Task<bool> IsAlive()
        {
            return Task.Run(() => { return true; });
        }
        public async Task<bool> SendLocationIsolationCrew(long outageId)
        {
            Logger.LogDebug("SendLocationIsolationCrew method started.");
            await InitAwaitableFields();
            OutageEntity outageEntity = null;
            List<OutageEntity> activeOutages = null;
            long reportedGid = 0;
            bool result = false;
            try
            {
                outageEntity = await outageModelAccessClient.GetOutage(outageId);
            }
            catch (Exception ex)
            {

                Logger.LogError($"OutageModel::SendLocationIsolationCrew => exception in UnitOfWork.OutageRepository.Get()", ex);
            }

            try
            {
                activeOutages = (await outageModelAccessClient.GetAllActiveOutages()).ToList();
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
                //if (task.IsCompleted)
                //{
                //    dbContext.Complete();
                //}
            }, TaskContinuationOptions.OnlyOnRanToCompletion).ContinueWith(async task =>
            {
                await outageLifecycleHelper.PublishOutage(Topic.ACTIVE_OUTAGE, outageMessageMapper.MapOutageEntity(outageEntity));

            }, TaskContinuationOptions.OnlyOnRanToCompletion);
            algorithm.Wait();

            return result;
        }
        public async Task<bool> StartLocationAndIsolationAlgorithm(OutageEntity outageEntity)
        {

            OutageTopologyElement topologyElement = null;

            long reportedGid = outageEntity.DefaultIsolationPoints.First().EquipmentId;
            long outageElementId = -1;
            long upBreaker;

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
                                    await outageModelAccessClient.AddOutage(entity);
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
                await outageModelAccessClient.RemoveOutage(outageEntity);
                Logger.LogError("End of feeder no outage detected.");
                return false;
            }
            outageModel.GetElementByGidFirstEnd(outageEntity.OutageElementGid, out topologyElement);
            while (topologyElement.DmsType != "BREAKER")
            {
                outageModel.GetElementByGidFirstEnd(topologyElement.Id, out topologyElement);
            }
            upBreaker = topologyElement.Id;
            long nextBreaker = outageLifecycleHelper.GetNextBreaker(outageEntity.OutageElementGid);

            if (!outageModel.OutageTopology.ContainsKey(nextBreaker))
            {
                string message = $"Breaker (next breaker) with id: 0x{nextBreaker:X16} is not in topology";
                Logger.LogError(message);
                throw new Exception(message);
            }
            long outageElement = outageModel.OutageTopology[nextBreaker].FirstEnd;

            if (!outageModel.OutageTopology[upBreaker].SecondEnd.Contains(outageElement))
            {
                string message = $"Outage element with gid: 0x{outageElement:X16} is not on a second end of current breaker id";
                Logger.LogError(message);
                throw new Exception(message);
            }
            outageEntity.OptimumIsolationPoints = await outageLifecycleHelper.GetEquipmentEntity(new List<long>() { upBreaker, nextBreaker });
            outageEntity.IsolatedTime = DateTime.UtcNow;
            outageEntity.OutageState = OutageState.ISOLATED;

            await outageModelAccessClient.UpdateOutage(outageEntity);
            await SendSCADACommand(upBreaker, DiscreteCommandingType.OPEN);
            await SendSCADACommand(nextBreaker, DiscreteCommandingType.OPEN);



            return true;
        }
        //TO DO use CE client
        private async Task SendSCADACommand(long currentBreakerId, DiscreteCommandingType discreteCommandingType)
        {
            long measurement = -1;

            List<long> measurements = new List<long>();

            try
            {
                measurements = await measurementMapServiceClient.GetMeasurementsOfElement(currentBreakerId);

            }
            catch (Exception e)
            {
                Logger.LogError("Error on GetMeasurementsForElement() method", e);
                throw e;
            }

            if (measurements.Count > 0)
            {
                measurement = measurements[0];
            }

            CommandedElements = await outageModelReadAccessClient.GetCommandedElements();

            if (measurement != -1)
            {
                if (discreteCommandingType == DiscreteCommandingType.OPEN && !CommandedElements.ContainsKey(currentBreakerId))
                {
                    await this.outageModelUpdateAccessClient.UpdateCommandedElements(currentBreakerId,ModelUpdateOperationType.INSERT);
                }


				try
				{
                    await switchStatusCommandingClient.SendOpenCommand(measurement);
				}
                catch (Exception e)
				{
                    Logger.LogError($"Error on SendOpenCommand method. Message: {e.Message}");

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

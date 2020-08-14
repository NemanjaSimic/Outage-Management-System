using Common.OMS;
using Common.OMS.OutageDatabaseModel;
using Common.OmsContracts.OutageLifecycle;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.NmsContracts;
using OMS.Common.NmsContracts.GDA;
using OMS.Common.PubSub;
using OMS.Common.PubSubContracts;
using OMS.Common.WcfClient.CE;
using OMS.Common.WcfClient.NMS;
using OMS.Common.WcfClient.OMS;
using OMS.Common.WcfClient.OMS.ModelAccess;
using OMS.OutageLifecycleServiceImplementation.OutageLCHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;
using AutoResetEvent = System.Threading.AutoResetEvent;
using OMS.OutageLifecycleServiceImplementation.ScadaSub;
using System.Fabric.Management.ServiceModel;
using OMS.Common.WcfClient.PubSub;
using OMS.Common.Cloud.Names;
using Common.OmsContracts.ModelProvider;
using Common.OmsContracts.ModelAccess;
using Common.CeContracts;

namespace OMS.OutageLifecycleServiceImplementation
{
	public class IsolateOutageService : IIsolateOutageContract
	{

		private IOutageTopologyModel outageModel;

		private OutageLifecycleHelper outageLifecycleHelper;
        private ModelResourcesDesc modelResourcesDesc;

        private Dictionary<long, long> CommandedElements;

        private ICloudLogger logger;

		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}

		#region Clients
		private IOutageModelReadAccessContract outageModelReadAccessClient;
        private IOutageModelUpdateAccessContract outageModelUpdateAccessClient;
        private IOutageAccessContract outageModelAccessClient;
        private IMeasurementMapContract measurementMapServiceClient;
        private ISwitchStatusCommandingContract switchStatusCommandingClient;
        private INetworkModelGDAContract networkModelGdaClient;
        private IEquipmentAccessContract equipmentAccessClient;
        private IRegisterSubscriberContract registerSubscriberClient;
        #endregion

        public ScadaSubscriber scadaSubscriber;
        public OutageMessageMapper outageMessageMapper;

		public IsolateOutageService(ScadaSubscriber scadaSubscriber)
		{
			this.outageModelReadAccessClient = OutageModelReadAccessClient.CreateClient();
            this.outageModelUpdateAccessClient = OutageModelUpdateAccessClient.CreateClient();
            this.outageModelAccessClient = OutageModelAccessClient.CreateClient();
            this.measurementMapServiceClient = MeasurementMapServiceClient.CreateClient();
            this.switchStatusCommandingClient = SwitchStatusCommandingClient.CreateClient();
            this.networkModelGdaClient = NetworkModelGdaClient.CreateClient();
            this.equipmentAccessClient = EquipmentAccessClient.CreateClient();
            this.registerSubscriberClient = RegisterSubscriberClient.CreateClient();

            this.scadaSubscriber = scadaSubscriber;

            this.outageMessageMapper = new OutageMessageMapper();
            this.modelResourcesDesc = new ModelResourcesDesc();

            CommandedElements = new Dictionary<long, long>();
        }

		

		public async Task InitAwaitableFields()
		{
			this.outageModel = await outageModelReadAccessClient.GetTopologyModel();
			this.outageLifecycleHelper = new OutageLifecycleHelper(this.outageModel);
		}

		public async Task IsolateOutage(long outageId)
		{
            Logger.LogDebug("IsolateOutage method started.");
            OutageEntity outageToIsolate = await outageModelAccessClient.GetOutage(outageId);

            if (outageToIsolate != null)
            {
                if (outageToIsolate.OutageState == OutageState.CREATED)
                {

                    await Task.Run(() => StartIsolationAlgorthm(outageToIsolate))
                        .ContinueWith(task =>
                        {
                            //if (task.Result)
                            //{
                            //    dbContext.Complete();
                            //}
                        }, TaskContinuationOptions.OnlyOnRanToCompletion)
                        .ContinueWith(async task =>
                        {
                            try
                            {
                                await outageLifecycleHelper.PublishOutage(Topic.ACTIVE_OUTAGE, outageMessageMapper.MapOutageEntity(outageToIsolate));
                                Logger.LogInformation($"Outage with id: 0x{outageToIsolate.OutageId:x16} is successfully published.");
                            }
                            catch (Exception e)
                            {
                            //TODO: mozda publish neke greske??
                            Logger.LogError("Error occured while trying to publish outage.", e);
                            }
                        }, TaskContinuationOptions.OnlyOnRanToCompletion);
                }
                else
                {
                    Logger.LogWarning($"Outage with id 0x{outageId:X16} is in state {outageToIsolate.OutageState}, and thus cannot be isolated.");
                }
            }
            else
            {
                Logger.LogWarning($"Outage with id 0x{outageId:X16} is not found in database.");
                //success = false;
            }

            //return success;
        }

        public async Task<bool> StartIsolationAlgorthm(OutageEntity outageToIsolate)
        {
            List<long> defaultIsolationPoints = outageToIsolate.DefaultIsolationPoints.Select(point => point.EquipmentId).ToList();

            bool isIsolated;
            bool isFirstBreakerRecloser;
            if (defaultIsolationPoints.Count > 0 && defaultIsolationPoints.Count < 3)
            {
                
                try
                {
                    isFirstBreakerRecloser = await CheckIfBreakerIsRecloser(defaultIsolationPoints[0]);
                    //TODO is second recloser (future)

                }
                catch (Exception e)
                {
                    Logger.LogWarning("Exception on method CheckIfBreakerIsRecloser()", e);
                    throw e;

                }

                long headBreaker = await GetHeadBreaker(defaultIsolationPoints, isFirstBreakerRecloser);
                long recloser = await GetRecloser(defaultIsolationPoints, isFirstBreakerRecloser);


                if (headBreaker != -1)
                {
                    ModelCode mc = modelResourcesDesc.GetModelCodeFromId(headBreaker);
                    if (mc == ModelCode.BREAKER)
                    {
                        long headBreakerMeasurementId, recloserMeasurementId;
                        
                        try
                        {
                            headBreakerMeasurementId = (await measurementMapServiceClient.GetMeasurementsOfElement(headBreaker))[0];
                            if (recloser != -1)
                            {
                                recloserMeasurementId = (await measurementMapServiceClient.GetMeasurementsOfElement(recloser))[0];
                            }
                            else
                            {
                                recloserMeasurementId = -1;
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.LogError("Error on GetMeasurementsForElement() method.", e);
                            throw e;
                        }
                        

                        Logger.LogInformation($"Head breaker id: 0x{headBreaker:X16}, recloser id: 0x{recloser:X16} (-1 if no recloser).");

                        //ALGORITHM
                        AutoResetEvent autoResetEvent = new AutoResetEvent(false);
                        CancelationObject cancelationObject = new CancelationObject() { CancelationSignal = false };
                        Timer timer = InitalizeAlgorthmTimer(cancelationObject, autoResetEvent);

                        scadaSubscriber.HeadBreakerID = headBreaker;
                        scadaSubscriber.AutoResetEvent = autoResetEvent;
                        //ScadaNotification scadaNotification = new ScadaNotification("OutageModel_SCADA_Subscriber", new OutageIsolationAlgorithm.OutageIsolationAlgorithmParameters(headBreakerMeasurementId, recloserMeasurementId, autoResetEvent));
                        //SubscriberProxy subscriberProxy = proxyFactory.CreateProxy<SubscriberProxy, ISubscriber>(scadaNotification, EndpointNames.SubscriberEndpoint);
                        //subscriberProxy.Subscribe(Topic.SWITCH_STATUS);
                        await registerSubscriberClient.SubscribeToTopic(Topic.SWITCH_STATUS, MicroserviceNames.OmsOutageLifecycleService);

                        long currentBreakerId = headBreaker;

                        while (!cancelationObject.CancelationSignal)
                        {
                            if ((await outageModelReadAccessClient.GetElementById(currentBreakerId)) != null)
                            {
                                currentBreakerId = GetNextBreaker(currentBreakerId);
                                Logger.LogDebug($"Next breaker is 0x{currentBreakerId:X16}.");

                                if (currentBreakerId == -1 || currentBreakerId == recloser)
                                {
                                    //TODO: planned outage
                                    string message = "End of the feeder, no outage detected.";
                                    Logger.LogWarning(message);
                                    isIsolated = false;
                                    //subscriberProxy.Close();
                                    await registerSubscriberClient.UnsubscribeFromAllTopics(MicroserviceNames.OmsOutageLifecycleService);
                                    await outageModelUpdateAccessClient.UpdateCommandedElements(0, ModelUpdateOperationType.CLEAR);
                                    //outageModel.commandedElements.Clear();
                                    throw new Exception(message);
                                }
                                //TODO: SCADACommand
                                await SendSCADACommand(currentBreakerId, DiscreteCommandingType.OPEN);
                                await SendSCADACommand(headBreaker, DiscreteCommandingType.CLOSE);

                                timer.Start();
                                Logger.LogDebug("Timer started.");
                                autoResetEvent.WaitOne();
                                if (timer.Enabled)
                                {
                                    timer.Stop();
                                    Logger.LogDebug("Timer stoped");
                                    await SendSCADACommand(currentBreakerId, DiscreteCommandingType.CLOSE);
                                }
                            }
                        }

                        long nextBreakerId = GetNextBreaker(currentBreakerId);
                        if (currentBreakerId != 0 && currentBreakerId != recloser)
                        {

                            Equipment headBreakerEquipment = await equipmentAccessClient.GetEquipment(headBreaker);
                            Equipment recloserEquipment = await equipmentAccessClient.GetEquipment(recloser);

                            if (recloserEquipment == null || headBreakerEquipment == null)
							{
                                string message = "Recloser or HeadBreaker were not found in database";
                                Logger.LogError(message);
                                throw new Exception(message);
							}
                            outageToIsolate.OptimumIsolationPoints = new List<Equipment>() { headBreakerEquipment, recloserEquipment };


                            if (!outageModel.OutageTopology.ContainsKey(nextBreakerId))
                            {
                                string message = $"Breaker (next breaker) with id: 0x{nextBreakerId:X16} is not in topology";
                                Logger.LogError(message);
                                throw new Exception(message);
                            }

                            long outageElement = outageModel.OutageTopology[nextBreakerId].FirstEnd;

                            if (!outageModel.OutageTopology[currentBreakerId].SecondEnd.Contains(outageElement))
                            {
                                string message = $"Outage element with gid: 0x{outageElement:X16} is not on a second end of current breaker id";
                                Logger.LogError(message);
                                throw new Exception(message);
                            }

                            await registerSubscriberClient.UnsubscribeFromAllTopics(MicroserviceNames.OmsOutageLifecycleService);
                            await outageModelUpdateAccessClient.UpdateOptimumIsolationPoints(currentBreakerId, ModelUpdateOperationType.INSERT);
                            await outageModelUpdateAccessClient.UpdateOptimumIsolationPoints(nextBreakerId, ModelUpdateOperationType.INSERT);

                            await SendSCADACommand(currentBreakerId, DiscreteCommandingType.OPEN);
                            await SendSCADACommand(nextBreakerId, DiscreteCommandingType.OPEN);

                            outageToIsolate.IsolatedTime = DateTime.UtcNow;
                            outageToIsolate.OutageElementGid = outageElement;
                            outageToIsolate.OutageState = OutageState.ISOLATED;

                            Logger.LogInformation($"Isolation of outage with id {outageToIsolate.OutageId}. Optimum isolation points: 0x{currentBreakerId:X16} and 0x{nextBreakerId:X16}, and outage element id is 0x{outageElement:X16}");
                            isIsolated = true;
                        }
                        else
                        {
                            string message = "End of the feeder, no outage detected.";
                            Logger.LogWarning(message);
                            isIsolated = false;
                            await registerSubscriberClient.UnsubscribeFromAllTopics(MicroserviceNames.OmsOutageLifecycleService);
                            await outageModelUpdateAccessClient.UpdateCommandedElements(0, ModelUpdateOperationType.CLEAR);
                            //outageModel.commandedElements.Clear();
                            throw new Exception(message);
                        }

                    }
                    else
                    {
                        Logger.LogWarning($"Head breaker type is {mc}, not a {ModelCode.BREAKER}.");
                        isIsolated = false;
                    }

                }
                else
                {
                    Logger.LogWarning("Head breaker not found.");
                    isIsolated = false;
                }
            }
            else
            {
                Logger.LogWarning($"Number of defaultIsolationPoints ({defaultIsolationPoints.Count}) is out of range [1, 2].");
                isIsolated = false;
            }


            await outageModelUpdateAccessClient.UpdateCommandedElements(0, ModelUpdateOperationType.CLEAR);
            return isIsolated;
        }

        public long GetNextBreaker(long breakerId)
        {
            if (!outageModel.OutageTopology.ContainsKey(breakerId))
            {
                string message = $"Breaker with gid: 0x{breakerId:X16} is not in a topology model.";
                Logger.LogError(message);
                throw new Exception(message);
            }

            long nextBreakerId = -1;

            foreach (long elementId in outageModel.OutageTopology[breakerId].SecondEnd)
            {
                if (modelResourcesDesc.GetModelCodeFromId(elementId) == ModelCode.ACLINESEGMENT)
                {
                    nextBreakerId = GetNextBreaker(elementId);
                }
                else if (modelResourcesDesc.GetModelCodeFromId(elementId) != ModelCode.BREAKER)
                {
                    return -1;
                }
                else
                {
                    return elementId;
                }

                if (nextBreakerId != -1)
                {
                    break;
                }
            }

            return nextBreakerId;
        }

        private async Task SendSCADACommand(long currentBreakerId, DiscreteCommandingType discreteCommandingType)
        {
            long measurement = -1;

            List<long> measurements = new List<long>();
            try
            {
                //TODO: see this??
                //measuremnts = measurementMapProxy.GetMeasurementsOfElement(currentBreakerId);
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
                    //TODO: add at list
                    await this.outageModelUpdateAccessClient.UpdateCommandedElements(currentBreakerId, ModelUpdateOperationType.INSERT);
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

                    //throw e;
                }

                /* using (SwitchStatusCommandingProxy scadaCommandProxy = proxyFactory.CreateProxy<SwitchStatusCommandingProxy, ISwitchStatusCommandingContract>(EndpointNames.SwitchStatusCommandingEndpoint))
                 {
                     try
                     {
                         scadaCommandProxy.SendOpenCommand(measurement);
                     }
                     catch (Exception e)
                     {
                         if (discreteCommandingType == DiscreteCommandingType.OPEN && CommandedElements.ContainsKey(currentBreakerId))
                         {
                             await this.outageModelUpdateAccessClient.UpdateCommandedElements(currentBreakerId,ModelUpdateOperationType.DELETE);
                         }
                         throw e;
                     }
                 }*/
            }

        }

        public async Task<bool> CheckIfBreakerIsRecloser(long elementId)
        {
            bool isRecloser = false;

            try
            {
                
                ResourceDescription resourceDescription = await networkModelGdaClient.GetValues(elementId, new List<ModelCode>() { ModelCode.BREAKER_NORECLOSING });

                Property property = resourceDescription.GetProperty(ModelCode.BREAKER_NORECLOSING);

                if (property != null)
                {
                    isRecloser = !property.AsBool();
                }
                else
                {
                    throw new Exception($"Element with id 0x{elementId:X16} is not a breaker.");
                }
                
            }
            catch (Exception e)
            {
                throw e;
            }

            return isRecloser;
        }

        private async Task<long> GetHeadBreaker(List<long> defaultIsolationPoints, bool isFirstBreakerRecloser)
		{
            long headBreaker = -1;
            if (defaultIsolationPoints.Count == 2)
            {
                if (isFirstBreakerRecloser)
                {
                    headBreaker = defaultIsolationPoints[1];
                }
                else
                {
                    headBreaker = defaultIsolationPoints[0];
                }
                await outageModelUpdateAccessClient.UpdateCommandedElements(headBreaker, ModelUpdateOperationType.INSERT);
            }
            else
            {
                if (!isFirstBreakerRecloser)
                {
                    headBreaker = defaultIsolationPoints[0];
                    await outageModelUpdateAccessClient.UpdateCommandedElements(headBreaker, ModelUpdateOperationType.INSERT);
                }
                else
                {
                    Logger.LogWarning($"Invalid state: breaker with id 0x{defaultIsolationPoints[0]:X16} is the only default isolation element, but it is also a recloser.");
                }
            }

            return headBreaker;
        }

        private async Task<long> GetRecloser(List<long> defaultIsolationPoints, bool isFirstBreakerRecloser)
        {
            long recloser = -1;
            if (defaultIsolationPoints.Count == 2)
            {
                if (isFirstBreakerRecloser)
                {
                    recloser = defaultIsolationPoints[0];
                }
                else
                {
                    recloser = defaultIsolationPoints[1];
                }
                await outageModelUpdateAccessClient.UpdateCommandedElements(recloser, ModelUpdateOperationType.INSERT);
            }
            

            return recloser;
        }


        private Timer InitalizeAlgorthmTimer(CancelationObject cancelationSignal, AutoResetEvent autoResetEvent)
        {
            Timer timer = new Timer();
            timer.Elapsed += (sender, e) => AlgorthmTimerElapsedCallback(sender, e, cancelationSignal, autoResetEvent);
            timer.Interval = 10000;
            timer.AutoReset = false;

            return timer;
        }

        private void AlgorthmTimerElapsedCallback(object sender, ElapsedEventArgs e, CancelationObject cancelationSignal, AutoResetEvent autoResetEvent)
        {
            Logger.LogDebug("Timer elapsed.");
            cancelationSignal.CancelationSignal = true;
            autoResetEvent.Set();
        }

    }
}

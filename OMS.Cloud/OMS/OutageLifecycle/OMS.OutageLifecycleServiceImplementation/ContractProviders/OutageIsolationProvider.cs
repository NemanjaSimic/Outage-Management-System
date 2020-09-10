using Common.OMS;
using Common.OmsContracts.DataContracts.OutageDatabaseModel;
using Common.OmsContracts.OutageLifecycle;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.NmsContracts;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using OMS.Common.WcfClient.CE;
using OMS.Common.WcfClient.OMS.ModelAccess;
using OMS.Common.WcfClient.OMS.ModelProvider;
using OMS.Common.WcfClient.SCADA;
using OMS.OutageLifecycleImplementation.Algorithm;
using OMS.OutageLifecycleImplementation.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OMS.OutageLifecycleImplementation.ContractProviders
{
    public class OutageIsolationProvider : IOutageIsolationContract
    {
        private readonly string baseLogString;
        private readonly OutageLifecycleHelper lifecycleHelper;
        private readonly ModelResourcesDesc modelResourcesDesc;
        private readonly OutageMessageMapper outageMessageMapper;
        private readonly IReliableStateManager stateManager;

        private ICloudLogger logger;

        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        #region Reliable Dictionaries
        private bool isMonitoredHeadBreakerMeasurementsInitialized;
        private bool isStartedIsolationAlgorithmsInitialized;

        private bool ReliableDictionariesInitialized
        {
            get
            {
                return isMonitoredHeadBreakerMeasurementsInitialized &&
                       isStartedIsolationAlgorithmsInitialized;
            }
        }

        private ReliableDictionaryAccess<long, DiscreteModbusData> monitoredHeadBreakerMeasurements;
        private ReliableDictionaryAccess<long, DiscreteModbusData> MonitoredHeadBreakerMeasurements
        {
            get { return monitoredHeadBreakerMeasurements; }
        }

        private ReliableDictionaryAccess<long, IsolationAlgorithm> startedIsolationAlgorithms;
        private ReliableDictionaryAccess<long, IsolationAlgorithm> StartedIsolationAlgorithms
        {
            get { return startedIsolationAlgorithms; }
        }

        private async void OnStateManagerChangedHandler(object sender, NotifyStateManagerChangedEventArgs e)
        {
            if (e.Action == NotifyStateManagerChangedAction.Add)
            {
                var operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
                string reliableStateName = operation.ReliableState.Name.AbsolutePath;

                if (reliableStateName == ReliableDictionaryNames.MonitoredHeadBreakerMeasurements)
                {
                    this.monitoredHeadBreakerMeasurements = await ReliableDictionaryAccess<long, DiscreteModbusData>.Create(stateManager, ReliableDictionaryNames.MonitoredHeadBreakerMeasurements);
                    this.isMonitoredHeadBreakerMeasurementsInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.MonitoredHeadBreakerMeasurements}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.StartedIsolationAlgorithms)
                {
                    this.startedIsolationAlgorithms = await ReliableDictionaryAccess<long, IsolationAlgorithm>.Create(stateManager, ReliableDictionaryNames.StartedIsolationAlgorithms);
                    this.isStartedIsolationAlgorithmsInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.StartedIsolationAlgorithms}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
            }
        }
        #endregion Reliable Dictionaries

        public OutageIsolationProvider(IReliableStateManager stateManager, OutageLifecycleHelper lifecycleHelper, ModelResourcesDesc modelResourcesDesc)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            this.lifecycleHelper = lifecycleHelper;
            this.modelResourcesDesc = modelResourcesDesc;
            this.outageMessageMapper = new OutageMessageMapper();

            this.isMonitoredHeadBreakerMeasurementsInitialized = false;
            this.isStartedIsolationAlgorithmsInitialized = false;

            this.stateManager = stateManager;
            this.stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;
        }

        #region IOutageIsolationContract
        public async Task IsolateOutage(long outageId)
        {
            Logger.LogDebug($"{baseLogString} IsolateOutage method started. OutageId: {outageId}");

            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            var result = await lifecycleHelper.GetCreatedOutage(outageId);

            if (!result.HasValue)
            {
                Logger.LogError($"{baseLogString} IsolateOutage => Created Outage is null. OutageId: {outageId}");
                return;
            }

            var outageToIsolate = result.Value;

            var startAlgorithmResult = await StartIsolationAlgorthm(outageToIsolate);

            if(startAlgorithmResult)
            {
                Logger.LogInformation($"{baseLogString} IsolateOutage => IsolationAlgorthm successfully started. OutageId: {outageId}");
            }

            //todo: PUBLUISH IN CYCLE
            //await lifecycleHelper.PublishOutageAsync(Topic.ACTIVE_OUTAGE, outageMessageMapper.MapOutageEntity(outageToIsolate));
            //Logger.LogInformation($"Outage with id: 0x{outageToIsolate.OutageId:x16} is successfully published.");
        }

        public Task<bool> IsAlive()
        {
            return Task.Run(() => { return true; });
        }
        #endregion IOutageIsolationContract

        #region Private Methods
        private async Task<bool> StartIsolationAlgorthm(OutageEntity outageToIsolate)
        {
            var result = await CreateIsolatingAlgorithm(outageToIsolate);

            if(!result.HasValue)
            {
                Logger.LogError($"{baseLogString} StartIsolationAlgorthm => CreateIsolatingAlgorithm did not return a vaule.");
                return false;
            }

            var algorithm = result.Value;

            Logger.LogInformation($"{baseLogString} StartIsolationAlgorthm => HeadBreakerGid: 0x{algorithm.HeadBreakerGid:X16}, RecloserGd: 0x{algorithm.RecloserGid:X16} (Recloser gid is -1 if there is no recloser...).");

            var scadaClient = ScadaIntegrityUpdateClient.CreateClient();
            var scadaPublication = await scadaClient.GetIntegrityUpdateForSpecificTopic(Topic.SWITCH_STATUS);

            if(!(scadaPublication.Message is MultipleDiscreteValueSCADAMessage multipleDiscreteValueSCADAMessage))
            {
                Logger.LogError($"{baseLogString} StartIsolationAlgorthm => ScadaPublication message is not of expected type: {typeof(MultipleDiscreteValueSCADAMessage)}, but of type: {scadaPublication.Message.GetType()}");
                return false;
            }

            if(!multipleDiscreteValueSCADAMessage.Data.ContainsKey(algorithm.HeadBreakerMeasurementGid))
            {
                Logger.LogError($"{baseLogString} StartIsolationAlgorthm => HeadBreakerMeasurement with gid: 0x{algorithm.HeadBreakerMeasurementGid:X16} not found in integrity update data received from scada.");
                return false;
            }

            var discreteModbusData = multipleDiscreteValueSCADAMessage.Data[algorithm.HeadBreakerMeasurementGid];
            await MonitoredHeadBreakerMeasurements.SetAsync(algorithm.HeadBreakerMeasurementGid, discreteModbusData);
            await StartedIsolationAlgorithms.SetAsync(algorithm.HeadBreakerGid, algorithm);

            return true;
        }

        private async Task<ConditionalValue<IsolationAlgorithm>> CreateIsolatingAlgorithm(OutageEntity outageToIsolate)
        {
            List<long> defaultIsolationPoints = outageToIsolate.DefaultIsolationPoints.Select(point => point.EquipmentId).ToList();

            if (defaultIsolationPoints.Count != 1 && defaultIsolationPoints.Count != 2)
            {
                Logger.LogWarning($"{baseLogString} CreateIsolatingAlgorithm => Number of defaultIsolationPoints ({defaultIsolationPoints.Count}) is out of range [1, 2].");
                return new ConditionalValue<IsolationAlgorithm>(false, null);
            }

            var measurementMapClient = MeasurementMapClient.CreateClient();
            bool isFirstBreakerRecloser = await lifecycleHelper.CheckIfBreakerIsRecloserAsync(defaultIsolationPoints[0]);

            #region HeadBreaker
            long headBreakerGid = await lifecycleHelper.GetHeadBreakerAsync(defaultIsolationPoints, isFirstBreakerRecloser);

            if (headBreakerGid <= 0)
            {
                Logger.LogWarning($"{baseLogString} CreateIsolatingAlgorithm => Head breaker not found.");
                return new ConditionalValue<IsolationAlgorithm>(false, null);
            }

            ModelCode headBreakerModelCode = modelResourcesDesc.GetModelCodeFromId(headBreakerGid);

            if (headBreakerModelCode != ModelCode.BREAKER)
            {
                Logger.LogError($"{baseLogString} CreateIsolatingAlgorithm => Head breaker type is {headBreakerModelCode}, not a {ModelCode.BREAKER}.");
                return new ConditionalValue<IsolationAlgorithm>(false, null);
            }

            var headBreakerMeasurementGid = (await measurementMapClient.GetMeasurementsOfElement(headBreakerGid)).FirstOrDefault();

            if (headBreakerMeasurementGid <= 0)
            {
                Logger.LogWarning($"{baseLogString} CreateIsolatingAlgorithm => Head breaker measurement not found.");
                return new ConditionalValue<IsolationAlgorithm>(false, null);
            }
            #endregion HeadBreaker

            #region Recloser
            long recloserGid = await lifecycleHelper.GetRecloserAsync(defaultIsolationPoints, isFirstBreakerRecloser);
            long recloserMeasurementGid = -1;

            if (recloserGid > 0) //ne mora postojati recloser
            {
                ModelCode recloserModelCode = modelResourcesDesc.GetModelCodeFromId(recloserGid);

                if (recloserModelCode != ModelCode.BREAKER)
                {
                    Logger.LogError($"{baseLogString} CreateIsolatingAlgorithm => Recloser type is {headBreakerModelCode}, not a {ModelCode.BREAKER}.");
                    return new ConditionalValue<IsolationAlgorithm>(false, null);
                }

                recloserMeasurementGid = (await measurementMapClient.GetMeasurementsOfElement(headBreakerGid)).FirstOrDefault();

                if (recloserMeasurementGid <= 0)
                {
                    Logger.LogWarning($"{baseLogString} CreateIsolatingAlgorithm => Head breaker measurement not found.");
                    return new ConditionalValue<IsolationAlgorithm>(false, null);
                }
            }
            #endregion Recloser

            var algorithm = new IsolationAlgorithm()
            {
                OutageId = outageToIsolate.OutageId,
                HeadBreakerGid = headBreakerGid,
                HeadBreakerMeasurementGid = headBreakerMeasurementGid,
                CurrentBreakerGid = headBreakerGid,
                CurrentBreakerMeasurementGid = headBreakerMeasurementGid,
                RecloserGid = recloserGid,
                RecloserMeasurementGid = recloserMeasurementGid,
                CycleCounter = 0,
            };

            return new ConditionalValue<IsolationAlgorithm>(true, algorithm);
        }

        private async Task<bool> OLD(OutageEntity outageToIsolate)
        {
            #region CreateIsolatingAlgorithm
            List<long> defaultIsolationPoints = outageToIsolate.DefaultIsolationPoints.Select(point => point.EquipmentId).ToList();

            if (defaultIsolationPoints.Count != 1 && defaultIsolationPoints.Count != 2)
            {
                Logger.LogWarning($"{baseLogString} StartIsolationAlgorthm => Number of defaultIsolationPoints ({defaultIsolationPoints.Count}) is out of range [1, 2].");
                return false;
            }

            bool isFirstBreakerRecloser = await lifecycleHelper.CheckIfBreakerIsRecloserAsync(defaultIsolationPoints[0]);
            long headBreakerGid = await lifecycleHelper.GetHeadBreakerAsync(defaultIsolationPoints, isFirstBreakerRecloser);
            long recloserGid = await lifecycleHelper.GetRecloserAsync(defaultIsolationPoints, isFirstBreakerRecloser);

            if (headBreakerGid == -1)
            {
                Logger.LogWarning($"{baseLogString} StartIsolationAlgorthm => Head breaker not found.");
                return false;
            }

            ModelCode mc = modelResourcesDesc.GetModelCodeFromId(headBreakerGid);

            if (mc != ModelCode.BREAKER)
            {
                Logger.LogWarning($"{baseLogString} StartIsolationAlgorthm => Head breaker type is {mc}, not a {ModelCode.BREAKER}.");
                return false;
            }

            //var measurementMapClient = MeasurementMapClient.CreateClient();
            //var headBreakerMeasurementGid = (await measurementMapClient.GetMeasurementsOfElement(headBreakerGid)).FirstOrDefault();

            //long recloserMeasurementGid = -1;

            //if (recloserGid != -1)
            //{
            //    recloserMeasurementGid = (await measurementMapClient.GetMeasurementsOfElement(recloserGid)).FirstOrDefault();
            //}

            Logger.LogInformation($"Head breaker id: 0x{headBreakerGid:X16}, recloser id: 0x{recloserGid:X16} (-1 if no recloser).");

            //ALGORITHM
            //TODO: set MONITROED HEAD BREAKER ID
            ///scadaSubscriber.HeadBreakerID = headBreaker;

            long currentBreakerGid = headBreakerGid;

            #endregion CreateIsolatingAlgorithm

            #region Cycle
            var outageModelReadAccessClient = OutageModelReadAccessClient.CreateClient();
            var outageModelUpdateAccessClient = OutageModelUpdateAccessClient.CreateClient();


            //while (!cancelationObject.CancelationSignal)
            //{ 
            //TODO: to CYCLE
            var topology = await outageModelReadAccessClient.GetTopologyModel();

            if ((await outageModelReadAccessClient.GetElementById(currentBreakerGid)) != null)
            {
                currentBreakerGid = lifecycleHelper.GetNextBreaker(currentBreakerGid, topology);
                Logger.LogDebug($"Next breaker is 0x{currentBreakerGid:X16}.");

                if (currentBreakerGid == -1 || currentBreakerGid == recloserGid)
                {
                    string message = "End of the feeder, no outage detected.";
                    Logger.LogWarning(message);
                    //TODO: end of - remove head id... await registerSubscriberClient.UnsubscribeFromAllTopics(MicroserviceNames.OmsOutageLifecycleService);
                    //var outageModelUpdateAccessClient = OutageModelUpdateAccessClient.CreateClient();
                    await outageModelUpdateAccessClient.UpdateCommandedElements(0, ModelUpdateOperationType.CLEAR);
                    //outageModel.commandedElements.Clear();
                    throw new Exception(message);
                }

                await lifecycleHelper.SendScadaCommandAsync(currentBreakerGid, DiscreteCommandingType.OPEN);
                await lifecycleHelper.SendScadaCommandAsync(headBreakerGid, DiscreteCommandingType.CLOSE);

                //timer.Start();
                //Logger.LogDebug("Timer started.");
                //autoResetEvent.WaitOne();
                //if (timer.Enabled)
                //{
                //    timer.Stop();
                //    Logger.LogDebug("Timer stoped");
                // 
                //todo: rethink... await SendScadaCommand(currentBreakerId, DiscreteCommandingType.CLOSE);
                //}
            }
            //}

            #endregion Cycle

            #region After Cycle
            long nextBreakerId = lifecycleHelper.GetNextBreaker(currentBreakerGid, topology);

            if (currentBreakerGid == 0 || currentBreakerGid == recloserGid)
            {
                string message = "End of the feeder, no outage detected.";
                Logger.LogWarning(message);
                //TODO: end of - remove head id... await registerSubscriberClient.UnsubscribeFromAllTopics(MicroserviceNames.OmsOutageLifecycleService);
                //var outageModelUpdateAccessClient = OutageModelUpdateAccessClient.CreateClient();
                await outageModelUpdateAccessClient.UpdateCommandedElements(0, ModelUpdateOperationType.CLEAR);
                //outageModel.commandedElements.Clear();
                throw new Exception(message);
            }

            var equipmentAccessClient = EquipmentAccessClient.CreateClient();
            Equipment headBreakerEquipment = await equipmentAccessClient.GetEquipment(headBreakerGid);
            Equipment recloserEquipment = await equipmentAccessClient.GetEquipment(recloserGid);

            if (recloserEquipment == null || headBreakerEquipment == null)
            {
                string message = "Recloser or HeadBreaker were not found in database";
                Logger.LogError(message);
                throw new Exception(message);
            }

            outageToIsolate.OptimumIsolationPoints = new List<Equipment>() { headBreakerEquipment, recloserEquipment };

            if (!topology.OutageTopology.ContainsKey(nextBreakerId))
            {
                string message = $"Breaker (next breaker) with id: 0x{nextBreakerId:X16} is not in topology";
                Logger.LogError(message);
                throw new Exception(message);
            }

            long outageElement = topology.OutageTopology[nextBreakerId].FirstEnd;

            if (!topology.OutageTopology[currentBreakerGid].SecondEnd.Contains(outageElement))
            {
                string message = $"Outage element with gid: 0x{outageElement:X16} is not on a second end of current breaker id";
                Logger.LogError(message);
                throw new Exception(message);
            }

            //TODO: end of - remove head id... await registerSubscriberClient.UnsubscribeFromAllTopics(MicroserviceNames.OmsOutageLifecycleService);
            //var outageModelUpdateAccessClient = OutageModelUpdateAccessClient.CreateClient();
            await outageModelUpdateAccessClient.UpdateOptimumIsolationPoints(currentBreakerGid, ModelUpdateOperationType.INSERT);
            await outageModelUpdateAccessClient.UpdateOptimumIsolationPoints(nextBreakerId, ModelUpdateOperationType.INSERT);

            await lifecycleHelper.SendScadaCommandAsync(currentBreakerGid, DiscreteCommandingType.OPEN);
            await lifecycleHelper.SendScadaCommandAsync(nextBreakerId, DiscreteCommandingType.OPEN);

            outageToIsolate.IsolatedTime = DateTime.UtcNow;
            outageToIsolate.OutageElementGid = outageElement;
            outageToIsolate.OutageState = OutageState.ISOLATED;

            Logger.LogInformation($"Isolation of outage with id {outageToIsolate.OutageId}. Optimum isolation points: 0x{currentBreakerGid:X16} and 0x{nextBreakerId:X16}, and outage element id is 0x{outageElement:X16}");

            //todo: this line goes after return TRUE from code above...
            //var outageModelUpdateAccessClient = OutageModelUpdateAccessClient.CreateClient();
            await outageModelUpdateAccessClient.UpdateCommandedElements(0, ModelUpdateOperationType.CLEAR);
            #endregion

            return isIsolated;
        }
        #endregion Private Methods
    }
}

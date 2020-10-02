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
using OMS.Common.WcfClient.SCADA;
using OMS.OutageLifecycleImplementation.Algorithm;
using OMS.OutageLifecycleImplementation.Helpers;
using System;
using System.Collections.Generic;
using System.Fabric;
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

        private async void OnStateManagerChangedHandler(object sender, NotifyStateManagerChangedEventArgs eventArgs)
        {
            try
            {
                await InitializeReliableCollections(eventArgs);
            }
            catch (FabricNotPrimaryException)
            {
                Logger.LogDebug($"{baseLogString} OnStateManagerChangedHandler => NotPrimaryException. To be ignored.");
            }
            catch (FabricObjectClosedException)
            {
                Logger.LogDebug($"{baseLogString} OnStateManagerChangedHandler => FabricObjectClosedException. To be ignored.");
            }
        }

        private async Task InitializeReliableCollections(NotifyStateManagerChangedEventArgs e)
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
            var ceModelProvider = CeModelProviderClient.CreateClient();
            bool isFirstBreakerRecloser = await ceModelProvider.IsRecloser(defaultIsolationPoints[0]);

            #region HeadBreaker
            long headBreakerGid = await GetHeadBreakerAsync(defaultIsolationPoints, isFirstBreakerRecloser);

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
            long recloserGid = await GetRecloserAsync(defaultIsolationPoints, isFirstBreakerRecloser);
            long recloserMeasurementGid = -1;

            if (recloserGid > 0) //ne mora postojati recloser
            {
                ModelCode recloserModelCode = modelResourcesDesc.GetModelCodeFromId(recloserGid);

                if (recloserModelCode != ModelCode.BREAKER)
                {
                    Logger.LogError($"{baseLogString} CreateIsolatingAlgorithm => Recloser type is {headBreakerModelCode}, not a {ModelCode.BREAKER}.");
                    return new ConditionalValue<IsolationAlgorithm>(false, null);
                }

                var ceModelProviderClient = CeModelProviderClient.CreateClient();
                if(!await ceModelProviderClient.IsRecloser(recloserGid))
                {
                    Logger.LogError($"{baseLogString} CreateIsolatingAlgorithm => Breaker with gid 0x{recloserGid:X16} is not a Recloser.");
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

        private async Task<long> GetHeadBreakerAsync(List<long> defaultIsolationPoints, bool isFirstBreakerRecloser)
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
            }
            else
            {
                if (!isFirstBreakerRecloser)
                {
                    headBreaker = defaultIsolationPoints[0];
                }
                else
                {
                    Logger.LogWarning($"Invalid state: breaker with id 0x{defaultIsolationPoints[0]:X16} is the only default isolation element, but it is also a recloser.");
                }
            }

            return headBreaker;
        }

        private async Task<long> GetRecloserAsync(List<long> defaultIsolationPoints, bool isFirstBreakerRecloser)
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
            }

            return recloser;
        }
        #endregion Private Methods
    }
}

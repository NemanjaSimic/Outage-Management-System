using Common.OMS;
using Common.OmsContracts.DataContracts.OutageDatabaseModel;
using Common.PubSubContracts.DataContracts.CE;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using OMS.Common.WcfClient.OMS.ModelAccess;
using OMS.Common.WcfClient.OMS.ModelProvider;
using OMS.OutageLifecycleImplementation.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.OutageLifecycleImplementation.Algorithm
{
    public class IsolationAlgorithmCycle
    {
        private readonly string baseLogString;
        private readonly IReliableStateManager stateManager;
        private readonly OutageLifecycleHelper lifecycleHelper;
        private readonly OutageMessageMapper outageMessageMapper;
        
        private ICloudLogger logger;

        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        #region Reliable Dictionaries
        private bool isStartedIsolationAlgorithmsInitialized;
        private bool isMonitoredHeadBreakerMeasurementsInitialized;

        private bool ReliableDictionariesInitialized
        {
            get
            {
                return isStartedIsolationAlgorithmsInitialized &&
                       isMonitoredHeadBreakerMeasurementsInitialized;
            }
        }

        private ReliableDictionaryAccess<long, IsolationAlgorithm> startedIsolationAlgorithms;
        private ReliableDictionaryAccess<long, IsolationAlgorithm> StartedIsolationAlgorithms
        {
            get { return startedIsolationAlgorithms; }
        }

        private ReliableDictionaryAccess<long, DiscreteModbusData> monitoredHeadBreakerMeasurements;
        private ReliableDictionaryAccess<long, DiscreteModbusData> MonitoredHeadBreakerMeasurements
        {
            get { return monitoredHeadBreakerMeasurements; }
        }

        private async void OnStateManagerChangedHandler(object sender, NotifyStateManagerChangedEventArgs e)
        {
            if (e.Action == NotifyStateManagerChangedAction.Add)
            {
                var operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
                string reliableStateName = operation.ReliableState.Name.AbsolutePath;

                if (reliableStateName == ReliableDictionaryNames.StartedIsolationAlgorithms)
                {
                    this.startedIsolationAlgorithms = await ReliableDictionaryAccess<long, IsolationAlgorithm>.Create(stateManager, ReliableDictionaryNames.StartedIsolationAlgorithms);
                    this.isStartedIsolationAlgorithmsInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.StartedIsolationAlgorithms}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if(reliableStateName == ReliableDictionaryNames.MonitoredHeadBreakerMeasurements)
                {
                    this.monitoredHeadBreakerMeasurements = await ReliableDictionaryAccess<long, DiscreteModbusData>.Create(stateManager, ReliableDictionaryNames.MonitoredHeadBreakerMeasurements);
                    this.isMonitoredHeadBreakerMeasurementsInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.MonitoredHeadBreakerMeasurements}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
            }
        }
        #endregion Reliable Dictionaries

        private readonly int cycleInterval;
        public int CycleInterval 
        { 
            get { return cycleInterval; }
        }

        private readonly int cycleUpperLimit;
        public int CycleUpperLimit
        {
            get { return cycleUpperLimit; }
        }

        public IsolationAlgorithmCycle(IReliableStateManager stateManager, OutageLifecycleHelper lifecycleHelper, int cycleInterval, int cycleUpperLimit)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            this.lifecycleHelper = lifecycleHelper;
            this.outageMessageMapper = new OutageMessageMapper();
            this.cycleInterval = cycleInterval;
            this.cycleUpperLimit = cycleUpperLimit;

            this.isStartedIsolationAlgorithmsInitialized = false;
            this.isMonitoredHeadBreakerMeasurementsInitialized = false;

            this.stateManager = stateManager;
            this.stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;
        }

        public async Task Start()
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            var outageModelReadAccessClient = OutageModelReadAccessClient.CreateClient();
            var topology = await outageModelReadAccessClient.GetTopologyModel();

            var tasks = new List<Task>();
            var enumerableStartedAlgorithms = await StartedIsolationAlgorithms.GetEnumerableDictionaryAsync();
            
            foreach(var algorithm in enumerableStartedAlgorithms.Values)
            {
                tasks.Add(StartIndividualAlgorithmCycle(algorithm, topology));
            }

            Task.WaitAll(tasks.ToArray());
        }


        private async Task StartIndividualAlgorithmCycle(IsolationAlgorithm algorithm, OutageTopologyModel topology)
        {
            //END CONDITION - poslednji otvoren brejker se nije otvorio vise od 'cycleUpperLimit' milisekundi => on predstavlja prvu optimalnu izolacionu tacku
            if (algorithm.CycleCounter * CycleInterval > CycleUpperLimit)
            {
                await FinishIndividualAlgorithmCycle(algorithm, topology);
                return;
            }

            #region Check if HeadBreaker has OPENED after the last cycle
            var result = await MonitoredHeadBreakerMeasurements.TryGetValueAsync(algorithm.HeadBreakerMeasurementGid);
            
            if(!result.HasValue)
            {
                Logger.LogError($"{baseLogString} StartIndividualAlgorithmCycle => HeadBreakerMeasurement with gid: 0x{algorithm.HeadBreakerMeasurementGid:X16} not found in {ReliableDictionaryNames.MonitoredHeadBreakerMeasurements}.");
                return;
            }

            var headMeasurementData = result.Value;
            
            if(headMeasurementData.Value == (ushort)DiscreteCommandingType.CLOSE)
            {
                //by exiting now we apply the logical WAITING (cycle mechanism in RunAsync):
                //1) For HeadBreaker to open => moving to next breaker
                //2) For "time" to run up => FinishIndividualAlgorithmCycle
                algorithm.CycleCounter++;
                return;
            }
            #endregion
            
            //Closing current breaker, before moving to the next breaker
            await lifecycleHelper.SendScadaCommandAsync(algorithm.CurrentBreakerGid, DiscreteCommandingType.CLOSE);
            algorithm.CycleCounter = 0;

            var outageModelReadAccessClient = OutageModelReadAccessClient.CreateClient();
            if ((await outageModelReadAccessClient.GetElementById(algorithm.CurrentBreakerGid)) == null) //todo: cemu ovaj uslov?
            {
                Logger.LogError($"{baseLogString} StartIndividualAlgorithmCycle => HeadBreakerMeasurement with gid: 0x{algorithm.HeadBreakerMeasurementGid:X16} not found in {ReliableDictionaryNames.MonitoredHeadBreakerMeasurements}.");
                return;
            }

            //moving to the next breaker
            algorithm.CurrentBreakerGid = lifecycleHelper.GetNextBreaker(algorithm.CurrentBreakerGid, topology);
            Logger.LogDebug($"{baseLogString} StartIndividualAlgorithmCycle => Next breaker gid is 0x{algorithm.CurrentBreakerGid:X16}.");

            //reaching the end of the feeder - ending the algorithm
            //TODO: see if algorithm is ended well for this case...
            if (algorithm.CurrentBreakerGid <= 0 || algorithm.CurrentBreakerGid == algorithm.RecloserGid)
            {
                await StartedIsolationAlgorithms.TryRemoveAsync(algorithm.HeadBreakerGid);
                await MonitoredHeadBreakerMeasurements.TryRemoveAsync(algorithm.HeadBreakerMeasurementGid);

                var outageModelUpdateAccessClient = OutageModelUpdateAccessClient.CreateClient();
                await outageModelUpdateAccessClient.UpdateCommandedElements(0, ModelUpdateOperationType.CLEAR); //TODO: zasto uvek skrnava resenja (ova 0 kao prvi parametar)? - treba izdvojiti posebnu metodu za clear...

                string message = $"{baseLogString} StartIndividualAlgorithmCycle => End of the feeder, no outage detected.";
                Logger.LogWarning(message);
                throw new Exception(message);
            }

            await lifecycleHelper.SendScadaCommandAsync(algorithm.CurrentBreakerGid, DiscreteCommandingType.OPEN);
            await lifecycleHelper.SendScadaCommandAsync(algorithm.HeadBreakerGid, DiscreteCommandingType.CLOSE);
            await StartedIsolationAlgorithms.SetAsync(algorithm.HeadBreakerGid, algorithm);
        }

        private async Task SetDefaultIsolationPoints(OutageEntity outageEntity, IsolationAlgorithm algorithm)
        {
            var equipmentAccessClient = EquipmentAccessClient.CreateClient();
            Equipment headBreakerEquipment = await equipmentAccessClient.GetEquipment(algorithm.HeadBreakerGid);
            Equipment recloserEquipment = await equipmentAccessClient.GetEquipment(algorithm.RecloserGid);

            if (headBreakerEquipment == null || recloserEquipment == null)
            {
                string message = $"{baseLogString} SetDefaultIsolationPoints => Recloser [0x{algorithm.HeadBreakerGid:X16}] or HeadBreaker [0x{algorithm.RecloserGid:X16}] were not found in database";
                Logger.LogError(message);
                throw new Exception(message);
            }

            outageEntity.DefaultIsolationPoints = new List<Equipment>() { headBreakerEquipment, recloserEquipment };
        }

        private async Task SetOptimumIsolationPoints(OutageEntity outageEntity, IsolationAlgorithm algorithm, OutageTopologyModel topology)
        {
            long firstOptimumIsolationPointGid = algorithm.CurrentBreakerGid;
            long secondOptimumIsolationPointGid = lifecycleHelper.GetNextBreaker(firstOptimumIsolationPointGid, topology);

            if (!topology.OutageTopology.ContainsKey(secondOptimumIsolationPointGid))
            {
                string message = $"{baseLogString} SetOptimumIsolationPoints => Breaker (next breaker) with id: 0x{secondOptimumIsolationPointGid:X16} is not in topology";
                Logger.LogError(message);
                throw new Exception(message);
            }

            long outageElement = topology.OutageTopology[secondOptimumIsolationPointGid].FirstEnd;

            if (!topology.OutageTopology[firstOptimumIsolationPointGid].SecondEnd.Contains(outageElement))
            {
                string message = $"{baseLogString} SetOptimumIsolationPoints => Outage element with gid: 0x{outageElement:X16} is not on a second end of current breaker id";
                Logger.LogError(message);
                throw new Exception(message);
            }

            var equipmentAccessClient = EquipmentAccessClient.CreateClient();
            Equipment firstOptimumIsolationPointEquipment = await equipmentAccessClient.GetEquipment(firstOptimumIsolationPointGid);
            Equipment secondOptimumIsolationPointEquipment = await equipmentAccessClient.GetEquipment(secondOptimumIsolationPointGid);

            if (firstOptimumIsolationPointEquipment == null || secondOptimumIsolationPointEquipment == null)
            {
                string message = $"{baseLogString} SetOptimumIsolationPoints => first OptimumIsolationPointGid [0x{firstOptimumIsolationPointGid:X16}] or second OptimumIsolationPointGid [0x{secondOptimumIsolationPointGid:X16}] were not found in database";
                Logger.LogError(message);
                throw new Exception(message);
            }

            //TODO: end of - remove head id... await registerSubscriberClient.UnsubscribeFromAllTopics(MicroserviceNames.OmsOutageLifecycleService);
            var outageModelUpdateAccessClient = OutageModelUpdateAccessClient.CreateClient();
            await outageModelUpdateAccessClient.UpdateOptimumIsolationPoints(firstOptimumIsolationPointGid, ModelUpdateOperationType.INSERT);
            await outageModelUpdateAccessClient.UpdateOptimumIsolationPoints(secondOptimumIsolationPointGid, ModelUpdateOperationType.INSERT);

            outageEntity.OptimumIsolationPoints = new List<Equipment>() { firstOptimumIsolationPointEquipment, secondOptimumIsolationPointEquipment };
        }

        private async Task FinishIndividualAlgorithmCycle(IsolationAlgorithm algorithm, OutageTopologyModel topology)
        {
            if (algorithm.CurrentBreakerGid <= 0 || algorithm.CurrentBreakerGid == algorithm.RecloserGid) //TODO; da li je suvisno imati ovu proveru?
            {
                await StartedIsolationAlgorithms.TryRemoveAsync(algorithm.HeadBreakerGid);
                await MonitoredHeadBreakerMeasurements.TryRemoveAsync(algorithm.HeadBreakerMeasurementGid);

                var outageModelUpdateAccessClientOnError = OutageModelUpdateAccessClient.CreateClient();
                await outageModelUpdateAccessClientOnError.UpdateCommandedElements(0, ModelUpdateOperationType.CLEAR); //TODO: zasto uvek skrnava resenja (ova 0 kao prvi parametar)? - treba izdvojiti posebnu metodu za clear...

                string message = $"{baseLogString} FinishIndividualAlgorithmCycle => End of the feeder, no outage detected.";
                Logger.LogWarning(message);
                throw new Exception(message);
            }

            var getCreatedOutageResult = await lifecycleHelper.GetCreatedOutage(algorithm.OutageId);

            if (!getCreatedOutageResult.HasValue)
            {
                Logger.LogError($"{baseLogString} FinishIndividualAlgorithmCycle => Created Outage is null. OutageId: {algorithm.OutageId}");
                return;
            }

            var outageToIsolate = getCreatedOutageResult.Value;
            await SetDefaultIsolationPoints(outageToIsolate, algorithm);
            await SetOptimumIsolationPoints(outageToIsolate, algorithm, topology);

            //ISOLATE on optimum points
            var firstOptimumPoint = outageToIsolate.OptimumIsolationPoints[0];
            var secondOptimumPoint = outageToIsolate.OptimumIsolationPoints[1];
            await lifecycleHelper.SendScadaCommandAsync(firstOptimumPoint.EquipmentId, DiscreteCommandingType.OPEN);
            await lifecycleHelper.SendScadaCommandAsync(secondOptimumPoint.EquipmentId, DiscreteCommandingType.OPEN);

            long outageElementGid = topology.OutageTopology[secondOptimumPoint.EquipmentId].FirstEnd;

            if (!topology.OutageTopology[firstOptimumPoint.EquipmentId].SecondEnd.Contains(outageElementGid))
            {
                string message = $"Outage element with gid: 0x{outageElementGid:X16} is not on a second end of current breaker id";
                Logger.LogError(message);
                throw new Exception(message);
            }

            outageToIsolate.IsolatedTime = DateTime.UtcNow;
            outageToIsolate.OutageElementGid = outageElementGid;
            outageToIsolate.OutageState = OutageState.ISOLATED;

            Logger.LogInformation($"{baseLogString} FinishIndividualAlgorithmCycle => Isolation of outage with id: {outageToIsolate.OutageId}. Optimum isolation points: 0x{outageToIsolate.OptimumIsolationPoints[0].EquipmentId:X16} and 0x{outageToIsolate.OptimumIsolationPoints[1].EquipmentId:X16}, and outage element id is 0x{outageElementGid:X16}");

            var outageModelUpdateAccessClient = OutageModelUpdateAccessClient.CreateClient();
            await outageModelUpdateAccessClient.UpdateCommandedElements(0, ModelUpdateOperationType.CLEAR);

            await lifecycleHelper.PublishOutageAsync(Topic.ACTIVE_OUTAGE, outageMessageMapper.MapOutageEntity(outageToIsolate));
            Logger.LogInformation($"{baseLogString} FinishIndividualAlgorithmCycle => Outage with id: 0x{outageToIsolate.OutageId:x16} is successfully published.");
        }

        //void Foo()
        //{
        //    long nextBreakerId = lifecycleHelper.GetNextBreaker(currentBreakerGid, topology);

        //    if (currentBreakerGid == 0 || currentBreakerGid == recloserGid)
        //    {
        //        string message = "End of the feeder, no outage detected.";
        //        Logger.LogWarning(message);
        //        //TODO: end of - remove head id... await registerSubscriberClient.UnsubscribeFromAllTopics(MicroserviceNames.OmsOutageLifecycleService);
        //        //var outageModelUpdateAccessClient = OutageModelUpdateAccessClient.CreateClient();
        //        await outageModelUpdateAccessClient.UpdateCommandedElements(0, ModelUpdateOperationType.CLEAR);
        //        //outageModel.commandedElements.Clear();
        //        throw new Exception(message);
        //    }

        //    var equipmentAccessClient = EquipmentAccessClient.CreateClient();
        //    Equipment headBreakerEquipment = await equipmentAccessClient.GetEquipment(headBreakerGid);
        //    Equipment recloserEquipment = await equipmentAccessClient.GetEquipment(recloserGid);

        //    if (recloserEquipment == null || headBreakerEquipment == null)
        //    {
        //        string message = "Recloser or HeadBreaker were not found in database";
        //        Logger.LogError(message);
        //        throw new Exception(message);
        //    }

        //    outageToIsolate.OptimumIsolationPoints = new List<Equipment>() { headBreakerEquipment, recloserEquipment };

        //    if (!topology.OutageTopology.ContainsKey(nextBreakerId))
        //    {
        //        string message = $"Breaker (next breaker) with id: 0x{nextBreakerId:X16} is not in topology";
        //        Logger.LogError(message);
        //        throw new Exception(message);
        //    }

        //    long outageElement = topology.OutageTopology[nextBreakerId].FirstEnd;

        //    if (!topology.OutageTopology[currentBreakerGid].SecondEnd.Contains(outageElement))
        //    {
        //        string message = $"Outage element with gid: 0x{outageElement:X16} is not on a second end of current breaker id";
        //        Logger.LogError(message);
        //        throw new Exception(message);
        //    }

        //    //TODO: end of - remove head id... await registerSubscriberClient.UnsubscribeFromAllTopics(MicroserviceNames.OmsOutageLifecycleService);
        //    //var outageModelUpdateAccessClient = OutageModelUpdateAccessClient.CreateClient();
        //    await outageModelUpdateAccessClient.UpdateOptimumIsolationPoints(currentBreakerGid, ModelUpdateOperationType.INSERT);
        //    await outageModelUpdateAccessClient.UpdateOptimumIsolationPoints(nextBreakerId, ModelUpdateOperationType.INSERT);

        //    await lifecycleHelper.SendScadaCommandAsync(currentBreakerGid, DiscreteCommandingType.OPEN);
        //    await lifecycleHelper.SendScadaCommandAsync(nextBreakerId, DiscreteCommandingType.OPEN);

        //    outageToIsolate.IsolatedTime = DateTime.UtcNow;
        //    outageToIsolate.OutageElementGid = outageElement;
        //    outageToIsolate.OutageState = OutageState.ISOLATED;

        //    Logger.LogInformation($"Isolation of outage with id {outageToIsolate.OutageId}. Optimum isolation points: 0x{currentBreakerGid:X16} and 0x{nextBreakerId:X16}, and outage element id is 0x{outageElement:X16}");

        //    //todo: this line goes after return TRUE from code above...
        //    //var outageModelUpdateAccessClient = OutageModelUpdateAccessClient.CreateClient();
        //    await outageModelUpdateAccessClient.UpdateCommandedElements(0, ModelUpdateOperationType.CLEAR);
        //}
    }
}

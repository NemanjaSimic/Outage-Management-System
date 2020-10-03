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
using OMS.OutageLifecycleImplementation.Helpers;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Runtime.InteropServices;
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
        private bool isOutageTopologyModelInitialized;
        private bool isOptimumIsolationPointsInitialized;
        private bool isCommandedElementsInitialized;
        private bool isElementsToBeIgnoredInReportPotentialOutageInitialized;

        private bool ReliableDictionariesInitialized
        {
            get
            {
                return true;
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

        private ReliableDictionaryAccess<string, OutageTopologyModel> outageTopologyModel;
        private ReliableDictionaryAccess<string, OutageTopologyModel> OutageTopologyModel
        {
            get { return outageTopologyModel; }
        }

        /// <summary>
        /// KEY - element gid of optimum isolation point
        /// VALUE - element gid of head switch (to identify the corresponding algorithm)
        /// </summary>
        private ReliableDictionaryAccess<long, long> optimumIsolationPoints;
        private ReliableDictionaryAccess<long, long> OptimumIsolationPoints
        {
            get { return optimumIsolationPoints; }
        }

        private ReliableDictionaryAccess<long, CommandedElement> commandedElements;
        private ReliableDictionaryAccess<long, CommandedElement> CommandedElements
        {
            get { return commandedElements; }
        }

        private ReliableDictionaryAccess<long, DateTime> elementsToBeIgnoredInReportPotentialOutage;
        private ReliableDictionaryAccess<long, DateTime> ElementsToBeIgnoredInReportPotentialOutage
        {
            get { return elementsToBeIgnoredInReportPotentialOutage; }
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
            catch (COMException)
            {
                Logger.LogDebug($"{baseLogString} OnStateManagerChangedHandler => {typeof(COMException)}. To be ignored.");
            }
        }

        private async Task InitializeReliableCollections(NotifyStateManagerChangedEventArgs e)
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
                else if (reliableStateName == ReliableDictionaryNames.MonitoredHeadBreakerMeasurements)
                {
                    this.monitoredHeadBreakerMeasurements = await ReliableDictionaryAccess<long, DiscreteModbusData>.Create(stateManager, ReliableDictionaryNames.MonitoredHeadBreakerMeasurements);
                    this.isMonitoredHeadBreakerMeasurementsInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.MonitoredHeadBreakerMeasurements}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.OutageTopologyModel)
                {
                    this.outageTopologyModel = await ReliableDictionaryAccess<string, OutageTopologyModel>.Create(stateManager, ReliableDictionaryNames.OutageTopologyModel);
                    this.isOutageTopologyModelInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.OutageTopologyModel}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.OptimumIsolationPoints)
                {
                    this.optimumIsolationPoints = await ReliableDictionaryAccess<long, long>.Create(stateManager, ReliableDictionaryNames.OptimumIsolationPoints);
                    this.isOptimumIsolationPointsInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.OptimumIsolationPoints}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.CommandedElements)
                {
                    this.commandedElements = await ReliableDictionaryAccess<long, CommandedElement>.Create(stateManager, ReliableDictionaryNames.CommandedElements);
                    this.isCommandedElementsInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.CommandedElements}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.ElementsToBeIgnoredInReportPotentialOutage)
                {
                    this.elementsToBeIgnoredInReportPotentialOutage = await ReliableDictionaryAccess<long, DateTime>.Create(stateManager, ReliableDictionaryNames.ElementsToBeIgnoredInReportPotentialOutage);
                    this.isElementsToBeIgnoredInReportPotentialOutageInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.ElementsToBeIgnoredInReportPotentialOutage}' ReliableDictionaryAccess initialized.";
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
            this.isOutageTopologyModelInitialized = false;
            this.isOptimumIsolationPointsInitialized = false;
            this.isCommandedElementsInitialized = false;
            this.isElementsToBeIgnoredInReportPotentialOutageInitialized = false;

            this.stateManager = stateManager;
            this.startedIsolationAlgorithms = new ReliableDictionaryAccess<long, IsolationAlgorithm>(stateManager, ReliableDictionaryNames.StartedIsolationAlgorithms);
            this.monitoredHeadBreakerMeasurements = new ReliableDictionaryAccess<long, DiscreteModbusData>(stateManager, ReliableDictionaryNames.MonitoredHeadBreakerMeasurements);
            this.outageTopologyModel = new ReliableDictionaryAccess<string, OutageTopologyModel>(stateManager, ReliableDictionaryNames.OutageTopologyModel);
            this.optimumIsolationPoints = new ReliableDictionaryAccess<long, long>(stateManager, ReliableDictionaryNames.OptimumIsolationPoints);
            this.commandedElements = new ReliableDictionaryAccess<long, CommandedElement>(stateManager, ReliableDictionaryNames.CommandedElements);
            this.elementsToBeIgnoredInReportPotentialOutage = new ReliableDictionaryAccess<long, DateTime>(stateManager, ReliableDictionaryNames.ElementsToBeIgnoredInReportPotentialOutage);
        }

        public async Task Start()
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            var enumerableStartedAlgorithms = await StartedIsolationAlgorithms.GetEnumerableDictionaryAsync();
            if(enumerableStartedAlgorithms.Count == 0)
            {
                Logger.LogVerbose($"{baseLogString} Start => No started algorithms.");
                return;
            }

            var enumerableTopology = await OutageTopologyModel.GetEnumerableDictionaryAsync();
            if (!enumerableTopology.ContainsKey(ReliableDictionaryNames.OutageTopologyModel))
            {
                Logger.LogError($"{baseLogString} Start => Topology not found in Rel Dictionary: {ReliableDictionaryNames.OutageTopologyModel}.");
                return;
            }

            var topology = enumerableTopology[ReliableDictionaryNames.OutageTopologyModel];
            var tasks = new List<Task<ConditionalValue<long>>>();
            
            foreach(var algorithm in enumerableStartedAlgorithms.Values)
            {
                tasks.Add(StartIndividualAlgorithmCycle(algorithm, topology));
            }

            var tasksArray = tasks.ToArray();
            Task.WaitAll(tasksArray);

            foreach(var task in tasksArray)
            {
                //SVESNO SE POGRESNO KORISTI HasValue
                if(!task.Result.HasValue)
                {
                    var headBreakerGid = task.Result.Value;
                    await OnEndAlgorithmCleanUp(headBreakerGid);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="algorithm"></param>
        /// <param name="topology"></param>
        /// <returns>ConditionalValue with HeadElementGid as value - HasValue: false indicates that task ended unsuccessfully, value will never be null and will represent the id of the task -> HeadElementGid</returns>
        private async Task<ConditionalValue<long>> StartIndividualAlgorithmCycle(IsolationAlgorithm algorithm, OutageTopologyModel topology)
        {
            var algorithmBaseLogString = $"{baseLogString} [HeadBreakerGid: 0x{algorithm.HeadBreakerGid:X16}]";

            //END CONDITION - poslednji otvoren brejker se nije otvorio vise od 'cycleUpperLimit' milisekundi => on predstavlja prvu optimalnu izolacionu tacku
            if (algorithm.CycleCounter * CycleInterval >= CycleUpperLimit)
            {
                Logger.LogInformation($"{algorithmBaseLogString} StartIndividualAlgorithmCycle => END CONDITION: {algorithm.CycleCounter} ms * {CycleInterval} ms > {CycleUpperLimit} ms.");
                var success = await FinishIndividualAlgorithmCycle(algorithm, topology);
                return new ConditionalValue<long>(success, algorithm.HeadBreakerGid);
            }
            else
            {
                Logger.LogInformation($"{algorithmBaseLogString} StartIndividualAlgorithmCycle => END CONDITION NOT MET: {algorithm.CycleCounter} ms * {CycleInterval} ms <= {CycleUpperLimit} ms.");
            }

            #region Check if HeadBreaker has OPENED after the last cycle
            var result = await MonitoredHeadBreakerMeasurements.TryGetValueAsync(algorithm.HeadBreakerMeasurementGid);
            
            if(!result.HasValue)
            {
                Logger.LogError($"{algorithmBaseLogString} StartIndividualAlgorithmCycle => HeadBreakerMeasurement with gid: 0x{algorithm.HeadBreakerMeasurementGid:X16} not found in {ReliableDictionaryNames.MonitoredHeadBreakerMeasurements}.");
                return new ConditionalValue<long>(false, algorithm.HeadBreakerGid);
            }

            var headMeasurementData = result.Value;
            
            if(headMeasurementData.Value == (ushort)DiscreteCommandingType.CLOSE)
            {
                Logger.LogInformation($"{algorithmBaseLogString} StartIndividualAlgorithmCycle => HeadBreaker is CLOSED.");

                //by exiting now we apply the logical WAITING (cycle mechanism in RunAsync):
                //1) For HeadBreaker to open => moving to next breaker
                //2) For "time" to run up => FinishIndividualAlgorithmCycle

                //counting cycles from after the command was successfully executed
                var commandsCount = await CommandedElements.GetCountAsync();
                if (commandsCount == 0)
                {
                    algorithm.CycleCounter++;
                    await StartedIsolationAlgorithms.SetAsync(algorithm.HeadBreakerGid, algorithm);
                    Logger.LogInformation($"{algorithmBaseLogString} StartIndividualAlgorithmCycle => AlgorithCycleCounter set to {algorithm.CycleCounter}.");
                }
                else
                {
                    Logger.LogInformation($"{algorithmBaseLogString} StartIndividualAlgorithmCycle => Skipping the cycle and waiting for {commandsCount} commands to be executed. HEAD is CLOSED. AlgorithCycleCounter remains set on {algorithm.CycleCounter}.");
                }

                return new ConditionalValue<long>(true, algorithm.HeadBreakerGid);
            }
            else if(headMeasurementData.Value == (ushort)DiscreteCommandingType.OPEN)
            {
                //skipping untill all commands were successfully executed
                var commandsCount = await CommandedElements.GetCountAsync();
                if (commandsCount > 0)
                {
                    Logger.LogInformation($"{algorithmBaseLogString} StartIndividualAlgorithmCycle => Skipping the cycle and waiting for {commandsCount} commands to be executed. HEAD is OPENED. AlgorithCycleCounter remains set on {algorithm.CycleCounter}.");
                    return new ConditionalValue<long>(true, algorithm.HeadBreakerGid);
                }
            }
            else
            {
                Logger.LogError($"{algorithmBaseLogString} StartIndividualAlgorithmCycle => headMeasurementData.Value is {headMeasurementData.Value} and cannot be casted to {typeof(DiscreteCommandingType)}.");
                return new ConditionalValue<long>(false, algorithm.HeadBreakerGid);
            }
            #endregion

            Logger.LogInformation($"{algorithmBaseLogString} StartIndividualAlgorithmCycle => HeadBreaker is OPENED.");

            //Closing current breaker, before moving to the next breaker
            var commands = new Dictionary<long, DiscreteCommandingType>();
            if(algorithm.CurrentBreakerGid != algorithm.HeadBreakerGid)
            {
                Logger.LogInformation($"{algorithmBaseLogString} StartIndividualAlgorithmCycle => Preaparing command to CLOSE the current breaker 0x{algorithm.CurrentBreakerGid:X16}");
                commands.Add(algorithm.CurrentBreakerGid, DiscreteCommandingType.CLOSE);
            }

            algorithm.CycleCounter = 0;

            //moving to the next breaker
            algorithm.CurrentBreakerGid = lifecycleHelper.GetNextBreaker(algorithm.CurrentBreakerGid, topology);
            Logger.LogDebug($"{algorithmBaseLogString} StartIndividualAlgorithmCycle => Next breaker gid is 0x{algorithm.CurrentBreakerGid:X16}.");

            if (algorithm.CurrentBreakerGid <= 0 || !topology.GetElementByGid(algorithm.CurrentBreakerGid, out _))
            {
                Logger.LogError($"{algorithmBaseLogString} StartIndividualAlgorithmCycle => HeadBreakerMeasurement with gid: 0x{algorithm.HeadBreakerMeasurementGid:X16} not found in {ReliableDictionaryNames.MonitoredHeadBreakerMeasurements}.");
                return new ConditionalValue<long>(false, algorithm.HeadBreakerGid);
            }

            var enumerableCommandedElements = await CommandedElements.GetEnumerableDictionaryAsync();

            //reaching the END of the FEEDER - ending the algorithm
            if (algorithm.CurrentBreakerGid == algorithm.RecloserGid)
            {
                string message = $"{algorithmBaseLogString} StartIndividualAlgorithmCycle => End of the feeder, no outage detected.";
                Logger.LogWarning(message);

                //TODO: HOW TO HANDEL - archived, deleted....
                await SendCommands(algorithm, commands, enumerableCommandedElements);
                return new ConditionalValue<long>(false, algorithm.HeadBreakerGid);
            }

            if(!commands.ContainsKey(algorithm.CurrentBreakerGid) && !commands.ContainsKey(algorithm.HeadBreakerGid))
            {
                commands.Add(algorithm.CurrentBreakerGid, DiscreteCommandingType.OPEN);
                Logger.LogInformation($"{algorithmBaseLogString} StartIndividualAlgorithmCycle => Preaparing command to OPEN the current breaker 0x{algorithm.CurrentBreakerGid:X16}");

                commands.Add(algorithm.HeadBreakerGid, DiscreteCommandingType.CLOSE);
                Logger.LogInformation($"{algorithmBaseLogString} StartIndividualAlgorithmCycle => Preaparing command to CLOSE the head breaker 0x{algorithm.HeadBreakerGid:X16}");
            }

            if(await SendCommands(algorithm, commands, enumerableCommandedElements))
            {
                Logger.LogInformation($"{algorithmBaseLogString} StartIndividualAlgorithmCycle => Commands sent with success.");
                return new ConditionalValue<long>(true, algorithm.HeadBreakerGid);
            }
            else
            {
                Logger.LogInformation($"{algorithmBaseLogString} StartIndividualAlgorithmCycle => Send commands failed.");
                return new ConditionalValue<long>(false, algorithm.HeadBreakerGid);
            }
        }

        private async Task<bool> SendCommands(IsolationAlgorithm algorithm, Dictionary<long, DiscreteCommandingType> commands, Dictionary<long, CommandedElement> commandedElements)
        {
            var algorithmBaseLogString = $"{baseLogString} [HeadBreakerGid: 0x{algorithm.HeadBreakerGid:X16}]";

            if (!await lifecycleHelper.SendMultipleScadaCommandAsync(commands, commandedElements, CommandOriginType.ISOLATING_ALGORITHM_COMMAND))
            {
                string message = $"{algorithmBaseLogString} StartIndividualAlgorithmCycle => Sending multiple command failed.";
                Logger.LogError(message);
                return false;
            }

            commands.Keys.ToList().ForEach(async commandedElementGid =>
            {
                var commandedElement = new CommandedElement()
                {
                    ElementGid = commandedElementGid,
                    CorrespondingHeadElementGid = algorithm.HeadBreakerGid,
                    CommandingType = commands[commandedElementGid],
                };

                await CommandedElements.SetAsync(commandedElementGid, commandedElement);
                await ElementsToBeIgnoredInReportPotentialOutage.SetAsync(commandedElementGid, DateTime.UtcNow);
                Logger.LogDebug($"{algorithmBaseLogString} SendCommands => Element 0x{commandedElementGid:X16} set to collection '{ReliableDictionaryNames.ElementsToBeIgnoredInReportPotentialOutage}' at {DateTime.UtcNow}.");
            });

            await StartedIsolationAlgorithms.SetAsync(algorithm.HeadBreakerGid, algorithm);
            return true;
        }

        private async Task<bool> FinishIndividualAlgorithmCycle(IsolationAlgorithm algorithm, OutageTopologyModel topology)
        {
            var algorithmBaseLogString = $"{baseLogString} [HeadBreakerGid: 0x{algorithm.HeadBreakerGid:X16}]";
            Logger.LogInformation($"{algorithmBaseLogString} entering FinishIndividualAlgorithmCycle.");

            if (algorithm.CurrentBreakerGid <= 0 || algorithm.CurrentBreakerGid == algorithm.RecloserGid)
            {
                string message = $"{algorithmBaseLogString} FinishIndividualAlgorithmCycle => End of the feeder, no outage detected.";
                Logger.LogWarning(message);
                return false;
            }

            var getCreatedOutageResult = await lifecycleHelper.GetCreatedOutage(algorithm.OutageId);

            if (!getCreatedOutageResult.HasValue)
            {
                Logger.LogError($"{algorithmBaseLogString} FinishIndividualAlgorithmCycle => Created Outage is null. OutageId: {algorithm.OutageId}");
                return false;
            }

            var outageToIsolate = getCreatedOutageResult.Value;
            await SetDefaultIsolationPoints(outageToIsolate, algorithm);
            await SetOptimumIsolationPoints(outageToIsolate, algorithm, topology);

            //ISOLATE on optimum points
            var firstOptimumPoint = outageToIsolate.OptimumIsolationPoints[0];
            var secondOptimumPoint = outageToIsolate.OptimumIsolationPoints[1];

            var commands = new Dictionary<long, DiscreteCommandingType>
            {
                { algorithm.HeadBreakerGid,         DiscreteCommandingType.CLOSE },
                { firstOptimumPoint.EquipmentId,    DiscreteCommandingType.OPEN },
                { secondOptimumPoint.EquipmentId,   DiscreteCommandingType.OPEN },
                { algorithm.RecloserGid,            DiscreteCommandingType.CLOSE },  
            };

            var enumerableCommandedElements = await CommandedElements.GetEnumerableDictionaryAsync();
            
            if (!await SendCommands(algorithm, commands, enumerableCommandedElements))
            {
                string message = $"{algorithmBaseLogString} FinishIndividualAlgorithmCycle => Failed on SendMultipleScadaCommandAsync.";
                Logger.LogError(message);
                return false;
            }

            long outageElementGid = topology.OutageTopology[secondOptimumPoint.EquipmentId].FirstEnd; //element iznad donjeg pointa - moze biti samo jedan gornji element (parent)

            if (!topology.OutageTopology[firstOptimumPoint.EquipmentId].SecondEnd.Contains(outageElementGid))
            {
                string message = $"{algorithmBaseLogString} FinishIndividualAlgorithmCycle => Outage element with gid: 0x{outageElementGid:X16} is not on a second end of current breaker id";
                Logger.LogError(message);
                return false;
            }

            outageToIsolate.IsolatedTime = DateTime.UtcNow;
            outageToIsolate.OutageElementGid = outageElementGid;
            outageToIsolate.OutageState = OutageState.ISOLATED;

            var outageModelAccessClient = OutageModelAccessClient.CreateClient();
            await outageModelAccessClient.UpdateOutage(outageToIsolate);

            Logger.LogInformation($"{algorithmBaseLogString} FinishIndividualAlgorithmCycle => Isolation of outage with id: {outageToIsolate.OutageId}. Optimum isolation points: 0x{outageToIsolate.OptimumIsolationPoints[0].EquipmentId:X16} and 0x{outageToIsolate.OptimumIsolationPoints[1].EquipmentId:X16}, and outage element id is 0x{outageElementGid:X16}");

            await lifecycleHelper.PublishOutageAsync(Topic.ACTIVE_OUTAGE, outageMessageMapper.MapOutageEntity(outageToIsolate));
            Logger.LogInformation($"{algorithmBaseLogString} FinishIndividualAlgorithmCycle => Outage with id: 0x{outageToIsolate.OutageId:x16} is successfully published.");

            await OnEndAlgorithmCleanUp(algorithm.HeadBreakerGid);
            return true;
        }

        //TODO: razmotriti stanje outage na ako se algoritam enduje sa nekim fejlom.... da li brisati sam outage npr...
        private async Task OnEndAlgorithmCleanUp(long headBreakerGid)
        {
            var algorithmBaseLogString = $"{baseLogString} [HeadBreakerGid: 0x{headBreakerGid:X16}]";

            await StartedIsolationAlgorithms.TryRemoveAsync(headBreakerGid);
            await MonitoredHeadBreakerMeasurements.TryRemoveAsync(headBreakerGid);

            var enumerableCommandedElements = await CommandedElements.GetEnumerableDictionaryAsync();
            var commandedElementsToBeRemoved = enumerableCommandedElements.Values.Where(element => element.CorrespondingHeadElementGid == headBreakerGid);

            foreach (var element in commandedElementsToBeRemoved)
            {
                await CommandedElements.TryRemoveAsync(element.ElementGid);
            }

            var enumerableOptimumIsolationPoints = await OptimumIsolationPoints.GetEnumerableDictionaryAsync();
            var optimumIsolationPointsToBeRemovedGids = enumerableOptimumIsolationPoints.Where(kvp => kvp.Value == headBreakerGid).Select(kvp => kvp.Key);

            foreach (var gid in optimumIsolationPointsToBeRemovedGids)
            {
                await OptimumIsolationPoints.TryRemoveAsync(gid);
            }
        }

        private async Task SetDefaultIsolationPoints(OutageEntity outageEntity, IsolationAlgorithm algorithm)
        {
            var algorithmBaseLogString = $"{baseLogString} [HeadBreakerGid: 0x{algorithm.HeadBreakerGid:X16}]";

            var equipmentAccessClient = EquipmentAccessClient.CreateClient();
            Equipment headBreakerEquipment = await equipmentAccessClient.GetEquipment(algorithm.HeadBreakerGid);
            Equipment recloserEquipment = await equipmentAccessClient.GetEquipment(algorithm.RecloserGid);

            if (headBreakerEquipment == null || recloserEquipment == null)
            {
                string message = $"{algorithmBaseLogString} SetDefaultIsolationPoints => Recloser [0x{algorithm.HeadBreakerGid:X16}] or HeadBreaker [0x{algorithm.RecloserGid:X16}] were not found in database";
                Logger.LogError(message);
                throw new Exception(message);
            }

            outageEntity.DefaultIsolationPoints = new List<Equipment>() { headBreakerEquipment, recloserEquipment };

            Logger.LogInformation($"{algorithmBaseLogString} SetDefaultIsolationPoints => Optimum points 0x{headBreakerEquipment:X16} and 0x{recloserEquipment:X16}");
        }

        private async Task SetOptimumIsolationPoints(OutageEntity outageEntity, IsolationAlgorithm algorithm, OutageTopologyModel topology)
        {
            var algorithmBaseLogString = $"{baseLogString} [HeadBreakerGid: 0x{algorithm.HeadBreakerGid:X16}]";

            long firstOptimumIsolationPointGid = algorithm.CurrentBreakerGid;
            long secondOptimumIsolationPointGid = lifecycleHelper.GetNextBreaker(firstOptimumIsolationPointGid, topology);

            if (!topology.OutageTopology.ContainsKey(secondOptimumIsolationPointGid))
            {
                string message = $"{algorithmBaseLogString} SetOptimumIsolationPoints => Breaker (next breaker) with id: 0x{secondOptimumIsolationPointGid:X16} is not in topology";
                Logger.LogError(message);
                throw new Exception(message);
            }

            long outageElement = topology.OutageTopology[secondOptimumIsolationPointGid].FirstEnd;

            if (!topology.OutageTopology[firstOptimumIsolationPointGid].SecondEnd.Contains(outageElement))
            {
                string message = $"{algorithmBaseLogString} SetOptimumIsolationPoints => Outage element with gid: 0x{outageElement:X16} is not on a second end of current breaker id";
                Logger.LogError(message);
                throw new Exception(message);
            }

            outageEntity.OptimumIsolationPoints = await lifecycleHelper.GetEquipmentEntityAsync(new List<long>() { firstOptimumIsolationPointGid, secondOptimumIsolationPointGid });

            if(outageEntity.OptimumIsolationPoints.Count != 2 || 
               !outageEntity.OptimumIsolationPoints.Any(point => point.EquipmentId == firstOptimumIsolationPointGid) || 
               !outageEntity.OptimumIsolationPoints.Any(point => point.EquipmentId == secondOptimumIsolationPointGid))
            {
                string message = $"{algorithmBaseLogString} SetOptimumIsolationPoints => first OptimumIsolationPointGid [0x{firstOptimumIsolationPointGid:X16}] or second OptimumIsolationPointGid [0x{secondOptimumIsolationPointGid:X16}] were not found or created successfully.";
                Logger.LogError(message);
                throw new Exception(message);
            }

            await OptimumIsolationPoints.SetAsync(firstOptimumIsolationPointGid, algorithm.HeadBreakerGid);
            await OptimumIsolationPoints.SetAsync(secondOptimumIsolationPointGid, algorithm.HeadBreakerGid);

            Logger.LogInformation($"{algorithmBaseLogString} SetOptimumIsolationPoints => Optimum points 0x{firstOptimumIsolationPointGid:X16} and 0x{secondOptimumIsolationPointGid:X16}");
        }
    }
}

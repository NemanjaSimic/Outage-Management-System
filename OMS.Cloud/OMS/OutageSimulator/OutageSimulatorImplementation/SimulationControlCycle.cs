using Common.OMS;
using Common.OMS.OutageSimulator;
using Common.OmsContracts.DataContracts.OutageSimulator;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using OMS.Common.WcfClient.CE;
using OMS.Common.WcfClient.SCADA;
using OMS.OutageSimulatorImplementation.DataContracts;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading.Tasks;

namespace OMS.OutageSimulatorImplementation
{
    public class SimulationControlCycle
    {
        private readonly string baseLogString;
        private readonly IReliableStateManager stateManager;

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        #region Reliable Dictionaries
        private bool isSimulatedOutagesInitialized;
        private bool isMonitoredIsolationPointsInitialized;
        private bool isCommandedValuesInitialized;

        private bool ReliableDictionariesInitialized
        {
            get
            {
                return isSimulatedOutagesInitialized &&
                       isMonitoredIsolationPointsInitialized &&
                       isCommandedValuesInitialized;
            }
        }

        private ReliableDictionaryAccess<long, SimulatedOutage> simulatedOutages;
        private ReliableDictionaryAccess<long, SimulatedOutage> SimulatedOutages
        {
            get { return simulatedOutages; }
        }

        private ReliableDictionaryAccess<long, MonitoredIsolationPoint> monitoredIsolationPoints;
        private ReliableDictionaryAccess<long, MonitoredIsolationPoint> MonitoredIsolationPoints
        {
            get { return monitoredIsolationPoints; }
        }

        private ReliableDictionaryAccess<long, CommandedValue> commandedValues;
        private ReliableDictionaryAccess<long, CommandedValue> CommandedValues
        {
            get { return commandedValues; }
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

                if (reliableStateName == ReliableDictionaryNames.SimulatedOutages)
                {
                    simulatedOutages = await ReliableDictionaryAccess<long, SimulatedOutage>.Create(stateManager, ReliableDictionaryNames.SimulatedOutages);
                    this.isSimulatedOutagesInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.SimulatedOutages}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.MonitoredIsolationPoints)
                {
                    monitoredIsolationPoints = await ReliableDictionaryAccess<long, MonitoredIsolationPoint>.Create(stateManager, ReliableDictionaryNames.MonitoredIsolationPoints);
                    this.isMonitoredIsolationPointsInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.MonitoredIsolationPoints}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.CommandedValues)
                {
                    commandedValues = await ReliableDictionaryAccess<long, CommandedValue>.Create(stateManager, ReliableDictionaryNames.CommandedValues);
                    this.isCommandedValuesInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.CommandedValues}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
            }
        }
        #endregion Private Propetires

        private readonly int commandedValueIntervalUpperLimit;
        public int CommandedValueIntervalUpperLimit
        {
            get { return commandedValueIntervalUpperLimit; }
        }

        public SimulationControlCycle(IReliableStateManager stateManager, int commandedValueIntervalUpperLimit)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            this.commandedValueIntervalUpperLimit = commandedValueIntervalUpperLimit;

            this.isSimulatedOutagesInitialized = false;
            this.isMonitoredIsolationPointsInitialized = false;

            this.stateManager = stateManager;
            this.stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;
        }

        public async Task Start()
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            var enumerableOutages = await SimulatedOutages.GetEnumerableDictionaryAsync();
            var enumerableMonitoredPoints = await MonitoredIsolationPoints.GetEnumerableDictionaryAsync();

            if(enumerableOutages.Count == 0 || enumerableMonitoredPoints.Count == 0)
            {
                Logger.LogVerbose($"{baseLogString} Start => No simulated outages. End of cycle.");
                return;
            }

            var defaultIsolationPointsToBeOpened = new Dictionary<long, DiscreteModbusData>();
            var measurementMapClient = MeasurementMapClient.CreateClient();
            var elementToMeasurementMap = await measurementMapClient.GetElementToMeasurementMap();

            foreach (var outage in enumerableOutages.Values)
            {
                foreach (long defaultPointElementGid in outage.DefaultIsolationPointGids)
                {
                    if (!elementToMeasurementMap.ContainsKey(defaultPointElementGid))
                    {
                        continue;
                    }

                    var defaultPointMeasurementGid = elementToMeasurementMap[defaultPointElementGid].FirstOrDefault();

                    if (!outage.DefaultToOptimumIsolationPointMap.ContainsKey(defaultPointElementGid))
                    {
                        continue;
                    }

                    long optimumPointElementGid = outage.DefaultToOptimumIsolationPointMap[defaultPointElementGid];

                    if(!elementToMeasurementMap.ContainsKey(optimumPointElementGid))
                    {
                        continue;
                    }

                    long optimumPointMeasurementGid = elementToMeasurementMap[optimumPointElementGid].FirstOrDefault();

                    if (!enumerableMonitoredPoints.ContainsKey(optimumPointMeasurementGid) || !enumerableMonitoredPoints.ContainsKey(defaultPointMeasurementGid))
                    {
                        continue;
                    }

                    var defaultIsolationPoint = enumerableMonitoredPoints[defaultPointMeasurementGid].DiscreteModbusData;
                    ushort optimumIsolationPointValue = enumerableMonitoredPoints[optimumPointMeasurementGid].DiscreteModbusData.Value;

                    if (optimumIsolationPointValue == (ushort)DiscreteCommandingType.CLOSE && defaultIsolationPoint.Value == (ushort)DiscreteCommandingType.CLOSE)
                    {
                        defaultIsolationPointsToBeOpened.Add(defaultPointMeasurementGid, defaultIsolationPoint);
                        Logger.LogInformation($"{baseLogString} Start => preparing command to OPEN 0x{defaultPointElementGid:X16} because element 0x{optimumPointElementGid:X16} is CLOSED.");
                    }
                }
            }

            await OpenIsolationPoints(defaultIsolationPointsToBeOpened);
        }

        #region Private Methods
        private async Task OpenIsolationPoints(Dictionary<long, DiscreteModbusData> isolationPoints)
        {
            var scadaCommandingClient = ScadaCommandingClient.CreateClient();
            var enumerableCommandedValues = await CommandedValues.GetEnumerableDictionaryAsync();

            foreach (long measurementGid in isolationPoints.Keys)
            {
                await UpdateCommandedValues(measurementGid, enumerableCommandedValues);

                var value = (ushort)DiscreteCommandingType.OPEN;

                if (isolationPoints[measurementGid].Value == (ushort)DiscreteCommandingType.CLOSE && await GetCommandedValuesCondition(measurementGid, value))
                {
                    await scadaCommandingClient.SendSingleDiscreteCommand(measurementGid,
                                                                          value,
                                                                          CommandOriginType.OUTAGE_SIMULATOR);

                    var commandedValue = new CommandedValue()
                    {
                        MeasurementGid = measurementGid,
                        Value = value,
                        TimeOfCreation = DateTime.UtcNow,
                    };

                    await CommandedValues.SetAsync(measurementGid, commandedValue);
                }
            }
        }

        private async Task UpdateCommandedValues(long measurementGid, Dictionary<long, CommandedValue> commandedValues)
        {
            if (commandedValues.ContainsKey(measurementGid))
            {
                var commandedValue = commandedValues[measurementGid];

                var difference = DateTime.UtcNow.Subtract(commandedValue.TimeOfCreation);

                if (difference.TotalMilliseconds >= CommandedValueIntervalUpperLimit)
                {
                    if ((await CommandedValues.TryRemoveAsync(measurementGid)).HasValue)
                    {
                        Logger.LogDebug($"{baseLogString} OpenIsolationPoints => Value succesfully set to scadaDataValue. Commanded value for gid 0x{measurementGid:X16} removed from rel dictionary '{ReliableDictionaryNames.CommandedValues}'.");
                    }
                }
            }
        }

        private async Task<bool> GetCommandedValuesCondition(long measurementGid, ushort commandValue)
        {
            var result = true;
            var enumerableCommandedValues = await CommandedValues.GetEnumerableDictionaryAsync();
            
            if(enumerableCommandedValues.ContainsKey(measurementGid))
            {
                var commandedValue = enumerableCommandedValues[measurementGid];

                if(commandedValue.Value == commandValue)
                {
                    result = false;
                }
            }

            return result;
        }
        #endregion Private Methods
    }
}

using Common.OMS;
using Common.OMS.OutageSimulator;
using Common.OmsContracts.DataContracts.OutageSimulator;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.PubSubContracts;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using OMS.Common.PubSubContracts.Interfaces;
using OMS.OutageSimulatorImplementation.DataContracts;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace OMS.OutageSimulatorImplementation.ContractProviders
{
    public class NotifySubscriberProvider : INotifySubscriberContract
    {
        private readonly string baseLogString;
        private readonly string subscriberName;
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
                return true;
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
            catch(FabricObjectClosedException)
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

        public NotifySubscriberProvider(IReliableStateManager stateManager, string subscriberName)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            this.subscriberName = subscriberName;

            this.isSimulatedOutagesInitialized = false;
            this.isMonitoredIsolationPointsInitialized = false;
            this.isCommandedValuesInitialized = false;

            this.stateManager = stateManager;
            this.simulatedOutages = new ReliableDictionaryAccess<long, SimulatedOutage>(stateManager, ReliableDictionaryNames.SimulatedOutages);
            this.monitoredIsolationPoints = new ReliableDictionaryAccess<long, MonitoredIsolationPoint>(stateManager, ReliableDictionaryNames.MonitoredIsolationPoints);
            this.commandedValues = new ReliableDictionaryAccess<long, CommandedValue>(stateManager, ReliableDictionaryNames.CommandedValues);
        }
        
        #region INotifySubscriberContract
        public async Task Notify(IPublishableMessage message, string publisherName)
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            if (!(message is MultipleDiscreteValueSCADAMessage multipleDiscreteValueSCADAMessage))
            {
                string errorMessage = $"SCADA returned wrong value for in SCADAPublication. {typeof(MultipleDiscreteValueSCADAMessage)} excepted.";
                Logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            var enumerableMonitoredPoints = await MonitoredIsolationPoints.GetEnumerableDictionaryAsync();
            var enumerableCommandedValues = await CommandedValues.GetEnumerableDictionaryAsync();

            foreach(long measurementGid in multipleDiscreteValueSCADAMessage.Data.Keys)
            {
                if(!enumerableMonitoredPoints.ContainsKey(measurementGid))
                {
                    continue;
                }

                var scadaDataValue = multipleDiscreteValueSCADAMessage.Data[measurementGid].Value;

                await UpdateMonitoredPoints(measurementGid, enumerableMonitoredPoints, scadaDataValue);

                await UpdateCommandedValues(measurementGid, enumerableCommandedValues, scadaDataValue);
            }
        }

        public Task<string> GetSubscriberName()
        {
            return Task.Run(() => subscriberName);
        }

        public Task<bool> IsAlive()
        {
            return Task.Run(() => true);
        }
        #endregion INotifySubscriberContract

        private async Task UpdateMonitoredPoints(long measurementGid, Dictionary<long, MonitoredIsolationPoint> enumerableMonitoredPoints, ushort scadaDataValue)
        {
            var monitoredPoint = enumerableMonitoredPoints[measurementGid];
            var monitoredPointData = monitoredPoint.DiscreteModbusData;

            if (scadaDataValue != monitoredPointData.Value)
            {
                var newDiscreteModbusData = new DiscreteModbusData(scadaDataValue,
                                                                   monitoredPointData.Alarm,
                                                                   monitoredPointData.MeasurementGid,
                                                                   monitoredPointData.CommandOrigin);

                monitoredPoint.DiscreteModbusData = newDiscreteModbusData;
                await MonitoredIsolationPoints.SetAsync(measurementGid, monitoredPoint);
            }
        }

        private async Task UpdateCommandedValues(long measurementGid, Dictionary<long, CommandedValue> enumerableCommandedValues, ushort scadaDataValue)
        {
            if (!enumerableCommandedValues.ContainsKey(measurementGid))
            {
                return;
            }

            if (enumerableCommandedValues[measurementGid].Value == scadaDataValue)
            {
                if ((await CommandedValues.TryRemoveAsync(measurementGid)).HasValue)
                {
                    Logger.LogDebug($"{baseLogString} Notify =>  Value succesfully set to scadaDataValue. Commanded value for gid 0x{measurementGid:X16} removed from rel dictionary '{ReliableDictionaryNames.CommandedValues}'.");
                }
            }
        }
    }
}

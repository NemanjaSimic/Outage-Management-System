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
using System;
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
        private bool ReliableDictionariesInitialized
        {
            get
            {
                return isSimulatedOutagesInitialized &&
                       isMonitoredIsolationPointsInitialized;
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

        private async void OnStateManagerChangedHandler(object sender, NotifyStateManagerChangedEventArgs e)
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
            }
        }
        #endregion Private Propetires

        public NotifySubscriberProvider(IReliableStateManager stateManager, string subscriberName)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            this.subscriberName = subscriberName;

            this.isSimulatedOutagesInitialized = false;
            this.isMonitoredIsolationPointsInitialized = false;

            this.stateManager = stateManager;
            this.stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;
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

            foreach(long gid in multipleDiscreteValueSCADAMessage.Data.Keys)
            {
                if(!enumerableMonitoredPoints.ContainsKey(gid))
                {
                    continue;
                }

                var scadaDataValue = multipleDiscreteValueSCADAMessage.Data[gid].Value;
                var monitoredPoint = enumerableMonitoredPoints[gid];
                var monitoredPointData = monitoredPoint.DiscreteModbusData;

                if (scadaDataValue != monitoredPointData.Value)
                {
                    var newDiscreteModbusData = new DiscreteModbusData(scadaDataValue,
                                                                       monitoredPointData.Alarm,
                                                                       monitoredPointData.MeasurementGid,
                                                                       monitoredPointData.CommandOrigin);

                    monitoredPoint.DiscreteModbusData = newDiscreteModbusData;
                    await MonitoredIsolationPoints.SetAsync(gid, monitoredPoint);
                }
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
    }
}

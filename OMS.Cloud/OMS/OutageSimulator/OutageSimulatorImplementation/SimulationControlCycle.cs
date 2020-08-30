using Common.OMS;
using Common.OMS.OutageSimulator;
using Common.OmsContracts.DataContracts.OutageSimulator;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using OMS.Common.WcfClient.SCADA;
using System;
using System.Collections.Generic;
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

        public SimulationControlCycle(IReliableStateManager stateManager)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

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

            var defaultIsolationPointsToBeOpened = new Dictionary<long, DiscreteModbusData>();
            var enumerableOutages = await SimulatedOutages.GetEnumerableDictionaryAsync();
            var enumerableMonitoredPoints = await MonitoredIsolationPoints.GetEnumerableDictionaryAsync();

            foreach(var outage in enumerableOutages.Values)
            {
                foreach (long defaultPointGid in outage.DefaultIsolationPointGids)
                {
                    if (!outage.DefaultToOptimumIsolationPointMap.ContainsKey(defaultPointGid))
                    {
                        continue;
                    }

                    long optimumPointGid = outage.DefaultToOptimumIsolationPointMap[defaultPointGid];

                    if (!enumerableMonitoredPoints.ContainsKey(optimumPointGid) || !enumerableMonitoredPoints.ContainsKey(defaultPointGid))
                    {
                        continue;
                    }

                    ushort optimumIsolationPointValue = enumerableMonitoredPoints[optimumPointGid].DiscreteModbusData.Value;

                    if (optimumIsolationPointValue == (ushort)DiscreteCommandingType.CLOSE)
                    {
                        defaultIsolationPointsToBeOpened.Add(defaultPointGid, enumerableMonitoredPoints[defaultPointGid].DiscreteModbusData);
                    }
                }
            }

            await OpenIsolationPoints(defaultIsolationPointsToBeOpened);
        }

        #region Private Methods
        private async Task OpenIsolationPoints(Dictionary<long, DiscreteModbusData> isolationPoints)
        {
            var scadaCommandingClient = ScadaCommandingClient.CreateClient();

            foreach (long gid in isolationPoints.Keys)
            {
                if (isolationPoints[gid].Value != (ushort)DiscreteCommandingType.OPEN)
                {
                    await scadaCommandingClient.SendSingleDiscreteCommand(gid, 
                                                                          (ushort)DiscreteCommandingType.OPEN,
                                                                          CommandOriginType.OUTAGE_SIMULATOR);
                }
            }
        }
        #endregion Private Methods
    }
}

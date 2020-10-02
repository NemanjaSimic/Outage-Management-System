using Common.OMS;
using Common.OMS.OutageSimulator;
using Common.OmsContracts.DataContracts.OutageSimulator;
using Common.OmsContracts.OutageSimulator;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.WcfClient.CE;
using System;
using System.Fabric;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace OMS.OutageSimulatorImplementation.ContractProviders
{
    public class OutageSimulatorProvider : IOutageSimulatorContract
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

        public OutageSimulatorProvider(IReliableStateManager stateManager)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            this.isSimulatedOutagesInitialized = false;
            this.isMonitoredIsolationPointsInitialized = false;

            this.stateManager = stateManager;
            this.stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;
        }

        #region IOutageSimulatorContract
        public async Task<bool> IsOutageElement(long outageElementId)
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            return await SimulatedOutages.ContainsKeyAsync(outageElementId);
        }

        public async Task<bool> StopOutageSimulation(long outageElementId)
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            if (!await SimulatedOutages.ContainsKeyAsync(outageElementId))
            {
                return false;
            }

            await RemoveNoLongerNeededMonitoredPoints(outageElementId);
            await SimulatedOutages.TryRemoveAsync(outageElementId);

            return true;
        }

        public Task<bool> IsAlive()
        {
            return Task.Run(() => true);
        }
        #endregion IOutageSimulatorContract

        private async Task RemoveNoLongerNeededMonitoredPoints(long outageElementGid)
        {
            var enumerableSimulatedOutages = await SimulatedOutages.GetEnumerableDictionaryAsync();

            if (!enumerableSimulatedOutages.ContainsKey(outageElementGid))
            {
                return;
            }

            var removeOutage = enumerableSimulatedOutages[outageElementGid];

            var measurementMapClient = MeasurementMapClient.CreateClient();
            var elementToMeasurementsMap = await measurementMapClient.GetElementToMeasurementMap();

            foreach (var monitoredElementGid in removeOutage.ElementsOfInteres)
            {
                bool stillNeeded = false;

                foreach (var outage in enumerableSimulatedOutages.Values)
                {
                    if (outage.OutageElementGid != removeOutage.OutageElementGid && outage.ElementsOfInteres.Contains(monitoredElementGid))
                    {
                        stillNeeded = true;
                        break;
                    }
                }

                if (!stillNeeded && elementToMeasurementsMap.ContainsKey(monitoredElementGid))
                {
                    var measurementGids = elementToMeasurementsMap[monitoredElementGid];
                    var measurementGid = measurementGids.FirstOrDefault();

                    await MonitoredIsolationPoints.TryRemoveAsync(measurementGid);
                }
            }
        }
    }
}

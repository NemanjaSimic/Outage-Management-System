using Common.OMS;
using Common.OMS.OutageSimulator;
using Common.OmsContracts.DataContracts.OutageSimulator;
using Common.OmsContracts.OutageSimulator;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using OMS.Common.WcfClient.CE;
using OMS.Common.WcfClient.SCADA;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading.Tasks;

namespace OMS.OutageSimulatorImplementation.ContractProviders
{
    public class OutageSimulatorUIProvider : IOutageSimulatorUIContract
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

        public OutageSimulatorUIProvider(IReliableStateManager stateManager)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            this.isSimulatedOutagesInitialized = false;
            this.isMonitoredIsolationPointsInitialized = false;

            this.stateManager = stateManager;
            this.stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;
        }

        #region IOutageSimulatorUIContract
        public async Task<IEnumerable<SimulatedOutage>> GetAllSimulatedOutages()
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            var outagesMap = await SimulatedOutages.GetDataCopyAsync();

            return outagesMap.Values; 
        }

        public async Task<bool> GenerateOutage(SimulatedOutage outage)
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            if(await SimulatedOutages.ContainsKeyAsync(outage.OutageElementGid))
            {
                return false;
            }

            await InitializeMonitoredPoints(outage);
            await SimulatedOutages.SetAsync(outage.OutageElementGid, outage);

            return true;
        }

        public async Task<bool> EndOutage(long outageElementGid)
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            if (!await SimulatedOutages.ContainsKeyAsync(outageElementGid))
            {
                return false;
            }

            await RemoveNoLongerNeededMonitoredPoints(outageElementGid);
            await SimulatedOutages.TryRemoveAsync(outageElementGid);

            return true;
        }

        public Task<bool> IsAlive()
        {
            return Task.Run(() => true);
        }
        #endregion IOutageSimulatorUIContract
    
        private async Task InitializeMonitoredPoints(SimulatedOutage outage)
        {
            var measurementMapClient = MeasurementMapClient.CreateClient();
            var measurementToElementGid = await measurementMapClient.GetMeasurementToElementMap();

            var scadaClient = ScadaIntegrityUpdateClient.CreateClient();
            var publication = await scadaClient.GetIntegrityUpdateForSpecificTopic(Common.Cloud.Topic.SWITCH_STATUS);

            if (!(publication.Message is MultipleDiscreteValueSCADAMessage multipleDiscreteValueSCADAMessage))
            {
                string errorMessage = $"SCADA returned wrong value for in SCADAPublication. {typeof(MultipleDiscreteValueSCADAMessage)} excepted.";
                Logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            foreach (var measurementGid in multipleDiscreteValueSCADAMessage.Data.Keys)
            {
                if(!measurementToElementGid.ContainsKey(measurementGid))
                {
                    continue;
                }

                var elementGid = measurementToElementGid[measurementGid];

                if (!outage.ElementsOfInteres.Contains(elementGid))
                {
                    continue;
                }

                var modbusData = multipleDiscreteValueSCADAMessage.Data[measurementGid];
                
                var monitoredPoint = new MonitoredIsolationPoint()
                {
                    IsolationElementGid = elementGid,
                    DiscreteMeasurementGid = measurementGid,
                    SimulatedOutageElementGid = outage.OutageElementGid,
                    DiscreteModbusData = modbusData,
                    IsolationPointType = outage.OptimumIsolationPointGids.Contains(elementGid) ?
                                                                         IsolationPointType.OPTIMUM :
                                                                         outage.DefaultIsolationPointGids.Contains(elementGid) ?
                                                                                                         IsolationPointType.DEFAULT :
                                                                                                         0,
                };

                await MonitoredIsolationPoints.SetAsync(measurementGid, monitoredPoint);
            }
        }

        private async Task RemoveNoLongerNeededMonitoredPoints(long outageElementGid)
        {
            var enumerableSimulatedOutages = await SimulatedOutages.GetEnumerableDictionaryAsync();
            
            if(!enumerableSimulatedOutages.ContainsKey(outageElementGid))
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

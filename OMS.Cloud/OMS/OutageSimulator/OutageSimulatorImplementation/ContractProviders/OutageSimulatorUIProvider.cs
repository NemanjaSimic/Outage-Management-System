using Common.OMS;
using Common.OMS.OutageSimulator;
using Common.OmsContracts.DataContracts.OutageSimulator;
using Common.OmsContracts.OutageSimulator;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using OMS.Common.WcfClient.SCADA;
using System;
using System.Collections.Generic;
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
            var scadaClient = ScadaIntegrityUpdateClient.CreateClient();
            var publication = await scadaClient.GetIntegrityUpdateForSpecificTopic(Common.Cloud.Topic.SWITCH_STATUS);

            if (!(publication.Message is MultipleDiscreteValueSCADAMessage multipleDiscreteValueSCADAMessage))
            {
                string errorMessage = $"SCADA returned wrong value for in SCADAPublication. {typeof(MultipleDiscreteValueSCADAMessage)} excepted.";
                Logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            foreach (long gid in multipleDiscreteValueSCADAMessage.Data.Keys)
            {
                if (!outage.PointsOfInteres.Contains(gid))
                {
                    continue;
                }

                var scadaPointData = multipleDiscreteValueSCADAMessage.Data[gid];
                var monitoredPoint = new MonitoredIsolationPoint()
                {
                    IsolationPointGid = gid,
                    SimulatedOutageElementGid = outage.OutageElementGid,
                    DiscreteModbusData = scadaPointData,
                    IsolationPointType = outage.OptimumIsolationPointGids.Contains(gid) ?
                                                                         IsolationPointType.OPTIMUM :
                                                                         outage.DefaultIsolationPointGids.Contains(gid) ?
                                                                                                         IsolationPointType.DEFAULT :
                                                                                                         0,
                };

                await MonitoredIsolationPoints.SetAsync(gid, monitoredPoint);
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

            foreach (var monitoredPoint in removeOutage.PointsOfInteres)
            {
                bool stillNeeded = false;

                foreach (var outage in enumerableSimulatedOutages.Values)
                {
                    if (outage.OutageElementGid != removeOutage.OutageElementGid && outage.PointsOfInteres.Contains(monitoredPoint))
                    {
                        stillNeeded = true;
                        break;
                    }
                }
                
                if(!stillNeeded)
                {
                    await MonitoredIsolationPoints.TryRemoveAsync(monitoredPoint);
                }
            }
        }
    }
}

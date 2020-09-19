using Common.OMS;
using Common.OmsContracts.DataContracts.OutageDatabaseModel;
using Common.OmsContracts.OutageLifecycle;
using Common.PubSubContracts.DataContracts.CE;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.WcfClient.OMS.ModelAccess;
using OMS.Common.WcfClient.OMS.ModelProvider;
using OMS.Common.WcfClient.OMS.OutageSimulator;
using OMS.OutageLifecycleImplementation.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OMS.OutageLifecycleImplementation.ContractProviders
{
    public class CrewSendingProvider : ICrewSendingContract
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
        private bool isOutageTopologyModelInitialized;

        private bool ReliableDictionariesInitialized
        {
            get
            {
                return isOutageTopologyModelInitialized;
            }
        }

        private ReliableDictionaryAccess<string, OutageTopologyModel> outageTopologyModel;
        private ReliableDictionaryAccess<string, OutageTopologyModel> OutageTopologyModel
        {
            get { return outageTopologyModel; }
        }

        private async void OnStateManagerChangedHandler(object sender, NotifyStateManagerChangedEventArgs e)
        {
            if (e.Action == NotifyStateManagerChangedAction.Add)
            {
                var operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
                string reliableStateName = operation.ReliableState.Name.AbsolutePath;

                if (reliableStateName == ReliableDictionaryNames.OutageTopologyModel)
                {
                    this.outageTopologyModel = await ReliableDictionaryAccess<string, OutageTopologyModel>.Create(stateManager, ReliableDictionaryNames.OutageTopologyModel);
                    this.isOutageTopologyModelInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.OutageTopologyModel}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
            }
        }
        #endregion Reliable Dictionaries

        public CrewSendingProvider(IReliableStateManager stateManager, OutageLifecycleHelper lifecycleHelper)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            this.lifecycleHelper = lifecycleHelper;
            this.outageMessageMapper = new OutageMessageMapper();

            this.isOutageTopologyModelInitialized = false;

            this.stateManager = stateManager;
            this.stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;
        }

        #region ICrewSendingContract
        public async Task<bool> SendLocationIsolationCrew(long outageId)
        {
            Logger.LogVerbose($"{baseLogString} SendLocationIsolationCrew method started. OutageId {outageId}");

            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            try
            {
                var result = await lifecycleHelper.GetCreatedOutage(outageId);

                if (!result.HasValue)
                {
                    Logger.LogError($"{baseLogString} SendLocationIsolationCrew => Created Outage is null. OutageId {outageId}");
                    return false;
                }

                var outageEntity = result.Value;

                var enumerableTopology = await OutageTopologyModel.GetEnumerableDictionaryAsync();

                if (!enumerableTopology.ContainsKey(ReliableDictionaryNames.OutageTopologyModel))
                {
                    Logger.LogError($"{baseLogString} Start => Topology not found in Rel Dictionary: {ReliableDictionaryNames.OutageTopologyModel}.");
                    return false;
                }

                var topology = enumerableTopology[ReliableDictionaryNames.OutageTopologyModel];

                if (!await StartLocationAndIsolationAlgorithm(outageEntity, topology))
                {
                    return false;
                }

                return await lifecycleHelper.PublishOutageAsync(Topic.ACTIVE_OUTAGE, outageMessageMapper.MapOutageEntity(outageEntity));
            }
            catch (Exception e)
            {
                string message = $"{baseLogString} SendLocationIsolationCrew => Exception: {e.Message}";
                Logger.LogError(message, e);

                return false;
            }
        }

        public async Task<bool> SendRepairCrew(long outageId)
        {
            Logger.LogDebug($"{baseLogString} SendRepairCrew method started. OutageId {outageId}");

            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            try
            {
                var result = await lifecycleHelper.GetIsolatedOutage(outageId);
            
                if(!result.HasValue)
                {
                    Logger.LogError($"{baseLogString} SendRepairCrew => Isolated Outage is null. OutageId {outageId}");
                    return false;
                }

                var outageEntity = result.Value;

                //TODO: WHY???
                //await Task.Delay(10000);

                var outageSimulatorClient = OutageSimulatorClient.CreateClient();

                if (!await outageSimulatorClient.StopOutageSimulation(outageEntity.OutageElementGid))
                { 
                    string message = $"{baseLogString} SendRepairCrew => StopOutageSimulation for element 0x{outageEntity.OutageElementGid:X16} failed. OutageId {outageId}";
                    Logger.LogError(message);
                    return false;
                }
            
                outageEntity.OutageState = OutageState.REPAIRED;
                outageEntity.RepairedTime = DateTime.UtcNow;

                var outageModelAccessClient = OutageModelAccessClient.CreateClient();
                await outageModelAccessClient.UpdateOutage(outageEntity);

                return await lifecycleHelper.PublishOutageAsync(Topic.ACTIVE_OUTAGE, this.outageMessageMapper.MapOutageEntity(outageEntity));
            }
            catch (Exception e)
            {
                string message = $"{baseLogString} SendRepairCrew => Exception: {e.Message}";
                Logger.LogError(message, e);

                return false;
            }
        }

        public Task<bool> IsAlive()
        {
            return Task.Run(() => { return true; });
        }
        #endregion ICrewSendingContract

        #region Private Methods
        private async Task<bool> StartLocationAndIsolationAlgorithm(OutageEntity outageEntity, OutageTopologyModel topology)
        {
            long reportedGid = outageEntity.DefaultIsolationPoints.First().EquipmentId;

            if (!topology.GetElementByGid(reportedGid, out OutageTopologyElement topologyElement))
            {
                Logger.LogError($"{baseLogString} StartLocationAndIsolationAlgorithm => element with gid 0x{reportedGid:X16} not found in outage topology model.");
                return false;
            }

            long upBreaker;
            long outageElementGid = -1;

            //MODO: tu je vec bio ovaj delay... zasto?
            //await Task.Delay(5000);

            var outageSimulatorClient = OutageSimulatorClient.CreateClient();
            var outageModelAccessClient = OutageModelAccessClient.CreateClient();

            //Da li je prijaveljen element OutageElement
            if (await outageSimulatorClient.IsOutageElement(reportedGid))
            {
                outageElementGid = reportedGid;
            }
            //Da li je mozda na ACL-novima ispod prijavljenog elementa
            else
            {
                for (int i = 0; i < topologyElement.SecondEnd.Count; i++)
                {
                    var potentialOutageElementGid = topologyElement.SecondEnd[i];

                    if (!(await outageSimulatorClient.IsOutageElement(potentialOutageElementGid)))
                    {
                        continue;
                    }

                    if (outageElementGid == -1)
                    {
                        outageElementGid = potentialOutageElementGid;
                        outageEntity.OutageElementGid = outageElementGid;
                    }
                    else
                    {
                        //KAKO SE ULAZI U OVAJ ELSE? => u else se ulazi tako sto se ide kroz for i prvi element se oznaci kao outage element, zatim se pronaje jos neki... znaci ovo je nacin da se kreira drugi, treci outage, na racvanju ispod elementa, po for-u....
                        var entity = new OutageEntity()
                        {
                            OutageElementGid = potentialOutageElementGid,
                            ReportTime = DateTime.UtcNow
                        };

                        await outageModelAccessClient.AddOutage(entity);
                    }
                }
            }

            //Tragamo za ACL-om gore ka source-u
            while (outageElementGid == -1 && !topologyElement.IsRemote && topologyElement.DmsType != "ENERGYSOURCE")
            {
                if (await outageSimulatorClient.IsOutageElement(topologyElement.Id))
                {
                    outageElementGid = topologyElement.Id;
                    outageEntity.OutageElementGid = outageElementGid;
                }

                topology.GetElementByGid(topologyElement.FirstEnd, out topologyElement);
            }

            if (outageElementGid == -1)
            {
                outageEntity.OutageState = OutageState.REMOVED;
                await outageModelAccessClient.RemoveOutage(outageEntity);

                Logger.LogError($"{baseLogString} StartLocationAndIsolationAlgorithm => End of feeder no outage detected.");
                return false;
            }

            topology.GetElementByGidFirstEnd(outageEntity.OutageElementGid, out topologyElement);
            while (topologyElement.DmsType != "BREAKER")
            {
                topology.GetElementByGidFirstEnd(topologyElement.Id, out topologyElement);
            }

            upBreaker = topologyElement.Id;
            long nextBreaker = lifecycleHelper.GetNextBreaker(outageEntity.OutageElementGid, topology);

            if (!topology.OutageTopology.ContainsKey(nextBreaker))
            {
                string message = $"Breaker (next breaker) with id: 0x{nextBreaker:X16} is not in topology";
                Logger.LogError(message);
                throw new Exception(message);
            }

            long outageElement = topology.OutageTopology[nextBreaker].FirstEnd;

            if (!topology.OutageTopology[upBreaker].SecondEnd.Contains(outageElement))
            {
                string message = $"Outage element with gid: 0x{outageElement:X16} is not on a second end of current breaker id";
                Logger.LogError(message);
                throw new Exception(message);
            }

            outageEntity.OptimumIsolationPoints = await lifecycleHelper.GetEquipmentEntityAsync(new List<long>() { upBreaker, nextBreaker });
            outageEntity.IsolatedTime = DateTime.UtcNow;
            outageEntity.OutageState = OutageState.ISOLATED;

            await outageModelAccessClient.UpdateOutage(outageEntity);
            await lifecycleHelper.SendScadaCommandAsync(upBreaker, DiscreteCommandingType.OPEN);
            await lifecycleHelper.SendScadaCommandAsync(nextBreaker, DiscreteCommandingType.OPEN);

            return true;
        }
        #endregion Private Methods
    }
}

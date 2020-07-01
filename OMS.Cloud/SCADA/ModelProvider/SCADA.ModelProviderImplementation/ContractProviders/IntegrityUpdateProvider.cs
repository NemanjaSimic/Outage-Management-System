using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Exceptions.SCADA;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts.DataContracts;
using OMS.Common.ScadaContracts.DataContracts.ScadaModelPointItems;
using OMS.Common.ScadaContracts.ModelProvider;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SCADA.ModelProviderImplementation.ContractProviders
{
    public class IntegrityUpdateProvider : IScadaIntegrityUpdateContract
    {
        private readonly string baseLogString;
        private readonly IReliableStateManager stateManager;

        #region Private Properties
        private bool isGidToPointItemMapInitialized;
        private bool isCommandDescriptionCacheInitialized;
        private bool isInfoCacheInitialized;
        private bool ReliableDictionariesInitialized
        {
            get { return isGidToPointItemMapInitialized && isCommandDescriptionCacheInitialized && isInfoCacheInitialized; }
        }

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        private ReliableDictionaryAccess<long, IScadaModelPointItem> gidToPointItemMap;
        private ReliableDictionaryAccess<long, IScadaModelPointItem> GidToPointItemMap
        {
            get
            {
                return gidToPointItemMap ?? (gidToPointItemMap = ReliableDictionaryAccess<long, IScadaModelPointItem>.Create(stateManager, ReliableDictionaryNames.GidToPointItemMap).Result);
            }
        }

        private ReliableDictionaryAccess<long, CommandDescription> commandDescriptionCache;
        private ReliableDictionaryAccess<long, CommandDescription> CommandDescriptionCache
        {
            get
            {
                return commandDescriptionCache ?? (commandDescriptionCache = ReliableDictionaryAccess<long, CommandDescription>.Create(stateManager, ReliableDictionaryNames.CommandDescriptionCache).Result);
            }
        }

        private ReliableDictionaryAccess<string, bool> infoCache;
        private ReliableDictionaryAccess<string, bool> InfoCache
        {
            get
            {
                return infoCache ?? (infoCache = ReliableDictionaryAccess<string, bool>.Create(stateManager, ReliableDictionaryNames.InfoCache).Result);
            }
        }
        #endregion Private Properties

        public IntegrityUpdateProvider(IReliableStateManager stateManager)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>";

            this.stateManager = stateManager;

            this.isGidToPointItemMapInitialized = false;
            this.isCommandDescriptionCacheInitialized = false;
            this.isInfoCacheInitialized = false;

            stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;
        }

        private async void OnStateManagerChangedHandler(object sender, NotifyStateManagerChangedEventArgs e)
        {
            if (e.Action == NotifyStateManagerChangedAction.Add)
            {
                var operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
                string reliableStateName = operation.ReliableState.Name.AbsolutePath;

                if (reliableStateName == ReliableDictionaryNames.GidToPointItemMap)
                {
                    //_ = GidToPointItemMap;
                    gidToPointItemMap = await ReliableDictionaryAccess<long, IScadaModelPointItem>.Create(stateManager, ReliableDictionaryNames.GidToPointItemMap);
                    this.isGidToPointItemMapInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.GidToPointItemMap}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.CommandDescriptionCache)
                {
                    //_ = CommandDescriptionCache;
                    commandDescriptionCache = await ReliableDictionaryAccess<long, CommandDescription>.Create(stateManager, ReliableDictionaryNames.CommandDescriptionCache);
                    this.isCommandDescriptionCacheInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.CommandDescriptionCache}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.InfoCache)
                {
                    //_ = InfoCache;
                    infoCache = await ReliableDictionaryAccess<string, bool>.Create(stateManager, ReliableDictionaryNames.InfoCache);
                    isInfoCacheInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.InfoCache}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
            }
        }

        private async Task<bool> GetIsScadaModelImportedIndicator()
        {
            while (!ReliableDictionariesInitialized)
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            string key = "IsScadaModelImported";
            if (!InfoCache.ContainsKey(key))
            {
                InfoCache[key] = false;
            }

            return InfoCache[key];
        }

        #region IScadaIntegrityUpdateContract
        public async Task<Dictionary<Topic, ScadaPublication>> GetIntegrityUpdate()
        {
            while (!ReliableDictionariesInitialized || !(await GetIsScadaModelImportedIndicator()))
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            if (GidToPointItemMap == null)
            {
                string message = $"GetIntegrityUpdate => GidToPointItemMap is null.";
                Logger.LogError(message);
                throw new InternalSCADAServiceException(message);
            }

            if (CommandDescriptionCache == null)
            {
                string message = $"GetIntegrityUpdate => CommandDescriptionCache is null.";
                Logger.LogError(message);
                throw new InternalSCADAServiceException(message);
            }

            Dictionary<long, AnalogModbusData> analogModbusData = new Dictionary<long, AnalogModbusData>();
            Dictionary<long, DiscreteModbusData> discreteModbusData = new Dictionary<long, DiscreteModbusData>();

            foreach (long gid in GidToPointItemMap.Keys)
            {
                CommandOriginType commandOrigin = CommandOriginType.OTHER_COMMAND;

                if (GidToPointItemMap[gid] is IAnalogPointItem analogPointItem)
                {
                    if (CommandDescriptionCache.ContainsKey(gid) && CommandDescriptionCache[gid].Value == analogPointItem.CurrentRawValue)
                    {
                        commandOrigin = CommandDescriptionCache[gid].CommandOrigin;
                    }

                    AnalogModbusData analogValue = new AnalogModbusData(analogPointItem.CurrentEguValue, analogPointItem.Alarm, gid, commandOrigin);
                    analogModbusData.Add(gid, analogValue);
                }
                else if (GidToPointItemMap[gid] is IDiscretePointItem discretePointItem)
                {
                    if (CommandDescriptionCache.ContainsKey(gid) && CommandDescriptionCache[gid].Value == discretePointItem.CurrentValue)
                    {
                        commandOrigin = CommandDescriptionCache[gid].CommandOrigin;
                    }

                    DiscreteModbusData discreteValue = new DiscreteModbusData(discretePointItem.CurrentValue, discretePointItem.Alarm, gid, commandOrigin);
                    discreteModbusData.Add(gid, discreteValue);
                }
            }

            MultipleAnalogValueSCADAMessage analogValuesMessage = new MultipleAnalogValueSCADAMessage(analogModbusData);
            ScadaPublication measurementPublication = new ScadaPublication(Topic.MEASUREMENT, analogValuesMessage);

            MultipleDiscreteValueSCADAMessage discreteValuesMessage = new MultipleDiscreteValueSCADAMessage(discreteModbusData);
            ScadaPublication switchStatusPublication = new ScadaPublication(Topic.SWITCH_STATUS, discreteValuesMessage);

            Dictionary<Topic, ScadaPublication> scadaPublications = new Dictionary<Topic, ScadaPublication>
            {
                { Topic.MEASUREMENT, measurementPublication },
                { Topic.SWITCH_STATUS, switchStatusPublication },
            };

            return scadaPublications;
        }

        public async Task<ScadaPublication> GetIntegrityUpdateForSpecificTopic(Topic topic)
        {
            while (!ReliableDictionariesInitialized || !(await GetIsScadaModelImportedIndicator()))
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            if (GidToPointItemMap == null)
            {
                string message = $"GetIntegrityUpdate => GidToPointItemMap is null.";
                Logger.LogError(message);
                throw new InternalSCADAServiceException(message);
            }

            if (CommandDescriptionCache == null)
            {
                string message = $"GetIntegrityUpdate => CommandDescriptionCache is null.";
                Logger.LogError(message);
                throw new InternalSCADAServiceException(message);
            }

            Dictionary<long, AnalogModbusData> analogModbusData = new Dictionary<long, AnalogModbusData>();
            Dictionary<long, DiscreteModbusData> discreteModbusData = new Dictionary<long, DiscreteModbusData>();

            foreach (long gid in GidToPointItemMap.Keys)
            {
                CommandOriginType commandOrigin = CommandOriginType.OTHER_COMMAND;

                if (topic == Topic.MEASUREMENT && GidToPointItemMap[gid] is IAnalogPointItem analogPointItem)
                {
                    if (CommandDescriptionCache.ContainsKey(gid) && CommandDescriptionCache[gid].Value == analogPointItem.CurrentRawValue)
                    {
                        commandOrigin = CommandDescriptionCache[gid].CommandOrigin;
                    }

                    AnalogModbusData analogValue = new AnalogModbusData(analogPointItem.CurrentEguValue, analogPointItem.Alarm, gid, commandOrigin);
                    analogModbusData.Add(gid, analogValue);
                }
                else if (topic == Topic.SWITCH_STATUS && GidToPointItemMap[gid] is IDiscretePointItem discretePointItem)
                {
                    if (CommandDescriptionCache.ContainsKey(gid) && CommandDescriptionCache[gid].Value == discretePointItem.CurrentValue)
                    {
                        commandOrigin = CommandDescriptionCache[gid].CommandOrigin;
                    }

                    DiscreteModbusData discreteValue = new DiscreteModbusData(discretePointItem.CurrentValue, discretePointItem.Alarm, gid, commandOrigin);
                    discreteModbusData.Add(gid, discreteValue);
                }
            }

            ScadaPublication scadaPublication;

            if (topic == Topic.MEASUREMENT)
            {
                MultipleAnalogValueSCADAMessage analogValuesMessage = new MultipleAnalogValueSCADAMessage(analogModbusData);
                scadaPublication = new ScadaPublication(Topic.MEASUREMENT, analogValuesMessage);

            }
            else if (topic == Topic.SWITCH_STATUS)
            {
                MultipleDiscreteValueSCADAMessage discreteValuesMessage = new MultipleDiscreteValueSCADAMessage(discreteModbusData);
                scadaPublication = new ScadaPublication(Topic.SWITCH_STATUS, discreteValuesMessage);
            }
            else
            {
                string message = $"GetIntegrityUpdate => argument topic is neither Topic.MEASUREMENT nor Topic.SWITCH_STATUS.";
                Logger.LogError(message);
                throw new ArgumentException(message);
            }

            return scadaPublication;
        }
        #endregion IScadaIntegrityUpdateContract
    }
}

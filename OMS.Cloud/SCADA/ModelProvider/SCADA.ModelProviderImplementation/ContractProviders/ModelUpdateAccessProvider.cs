using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Exceptions.SCADA;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.PubSub;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using OMS.Common.ScadaContracts.DataContracts;
using OMS.Common.ScadaContracts.DataContracts.ScadaModelPointItems;
using OMS.Common.ScadaContracts.ModelProvider;
using OMS.Common.WcfClient.PubSub;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ReliableDictionaryNames = OMS.Common.SCADA.ReliableDictionaryNames;

namespace SCADA.ModelProviderImplementation.ContractProviders
{
    public class ModelUpdateAccessProvider : IScadaModelUpdateAccessContract
    {
        private readonly IReliableStateManager stateManager;
        private readonly PublisherClient publisherClient;

        private bool isGidToPointItemMapInitialized;
        private bool isMeasurementsCacheInitialized;
        private bool isCommandDescriptionCacheInitialized;
        private bool isInfoCacheInitialized;

        #region Private Propetires
        private bool ReliableDictionariesInitialized
        {
            get { return isGidToPointItemMapInitialized && isMeasurementsCacheInitialized && isCommandDescriptionCacheInitialized && isInfoCacheInitialized; }
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

        private ReliableDictionaryAccess<long, ModbusData> measurementsCache;
        private ReliableDictionaryAccess<long, ModbusData> MeasurementsCache
        {
            get
            {
                return measurementsCache ?? (measurementsCache = ReliableDictionaryAccess<long, ModbusData>.Create(stateManager, ReliableDictionaryNames.MeasurementsCache).Result);
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
        #endregion Private Propetires

        public ModelUpdateAccessProvider(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
            stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;

            this.publisherClient = PublisherClient.CreateClient();

            this.isGidToPointItemMapInitialized = false;
            this.isMeasurementsCacheInitialized = false;
            this.isCommandDescriptionCacheInitialized = false;
            this.isInfoCacheInitialized = false;
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
                }
                else if (reliableStateName == ReliableDictionaryNames.MeasurementsCache)
                {
                    //_ = MeasurementsCache;
                    measurementsCache = await ReliableDictionaryAccess<long, ModbusData>.Create(stateManager, ReliableDictionaryNames.MeasurementsCache);
                    this.isMeasurementsCacheInitialized = true;
                }
                else if (reliableStateName == ReliableDictionaryNames.CommandDescriptionCache)
                {
                    //_ = CommandDescriptionCache;
                    commandDescriptionCache = await ReliableDictionaryAccess<long, CommandDescription>.Create(stateManager, ReliableDictionaryNames.CommandDescriptionCache);
                    this.isCommandDescriptionCacheInitialized = true;
                }
                else if (reliableStateName == ReliableDictionaryNames.InfoCache)
                {
                    //_ = InfoCache;
                    infoCache = await ReliableDictionaryAccess<string, bool>.Create(stateManager, ReliableDictionaryNames.InfoCache);
                    isInfoCacheInitialized = true;
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

        #region IScadaModelUpdateAccessContract
        public async Task MakeAnalogEntryToMeasurementCache(Dictionary<long, AnalogModbusData> data, bool permissionToPublishData)
        {
            while (!ReliableDictionariesInitialized || !(await GetIsScadaModelImportedIndicator()))
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            Dictionary<long, AnalogModbusData> publicationData = new Dictionary<long, AnalogModbusData>();

            if (data == null)
            {
                string message = $"WriteToMeasurementsCache() => readAnalogCommand.Data is null.";
                Logger.LogError(message);
                throw new NullReferenceException(message);
            }

            if (MeasurementsCache == null)
            {
                string message = $"GetIntegrityUpdate => gidToPointItemMap is null.";
                Logger.LogError(message);
                throw new InternalSCADAServiceException(message);
            }

            foreach (long gid in data.Keys)
            {
                if (!MeasurementsCache.ContainsKey(gid))
                {
                    await MeasurementsCache.SetAsync(gid, data[gid]);
                    publicationData[gid] = data[gid];
                }
                else if (MeasurementsCache[gid] is AnalogModbusData analogCacheItem && analogCacheItem.Value != data[gid].Value)
                {
                    Logger.LogDebug($"Value changed on element with id: {analogCacheItem.MeasurementGid}. Old value: {analogCacheItem.Value}; new value: {data[gid].Value}");

                    MeasurementsCache[gid] = data[gid];
                    publicationData[gid] = MeasurementsCache[gid] as AnalogModbusData;
                }
            }

            //if data is empty that means that there are no new values in the current acquisition cycle
            if (permissionToPublishData && publicationData.Count > 0)
            {
                ScadaMessage scadaMessage = new MultipleAnalogValueSCADAMessage(publicationData);
                PublishScadaData(Topic.MEASUREMENT, scadaMessage);
            }
        }

        public async Task MakeDiscreteEntryToMeasurementCache(Dictionary<long, DiscreteModbusData> data, bool permissionToPublishData)
        {
            while (!ReliableDictionariesInitialized || !(await GetIsScadaModelImportedIndicator()))
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            Dictionary<long, DiscreteModbusData> publicationData = new Dictionary<long, DiscreteModbusData>();

            if (data == null)
            {
                string message = $"WriteToMeasurementsCache() => readAnalogCommand.Data is null.";
                Logger.LogError(message);
                throw new NullReferenceException(message);
            }

            if (MeasurementsCache == null)
            {
                string message = $"GetIntegrityUpdate => gidToPointItemMap is null.";
                Logger.LogError(message);
                throw new InternalSCADAServiceException(message);
            }

            foreach (long gid in data.Keys)
            {
                if (!MeasurementsCache.ContainsKey(gid))
                {
                    await MeasurementsCache.SetAsync(gid, data[gid]);
                    publicationData[gid] = data[gid];
                }
                else if (MeasurementsCache[gid] is DiscreteModbusData discreteCacheItem && discreteCacheItem.Value != data[gid].Value)
                {
                    Logger.LogDebug($"Value changed on element with id :{discreteCacheItem.MeasurementGid};. Old value: {discreteCacheItem.Value}; new value: {data[gid].Value}");

                    MeasurementsCache[gid] = data[gid];
                    publicationData[gid] = MeasurementsCache[gid] as DiscreteModbusData;
                }
            }

            //if data is empty that means that there are no new values in the current acquisition cycle
            if (permissionToPublishData && publicationData.Count > 0)
            {
                ScadaMessage scadaMessage = new MultipleDiscreteValueSCADAMessage(publicationData);
                PublishScadaData(Topic.SWITCH_STATUS, scadaMessage);
            }
        }

        public async Task<IScadaModelPointItem> UpdatePointItemRawValue(long gid, int rawValue)
        {
            while (!ReliableDictionariesInitialized || !(await GetIsScadaModelImportedIndicator()))
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            if (!GidToPointItemMap.ContainsKey(gid))
            {
                string message = $"UpdatePointItemRawValue => Entity with Gid: 0x{gid:X16} does not exist in GidToPointItemMap.";
                Logger.LogError(message);
                throw new ArgumentException(message);
            }

            if (GidToPointItemMap[gid] is IAnalogPointItem analogPoint)
            {
                analogPoint.CurrentEguValue = analogPoint.RawToEguValueConversion(rawValue);
                await GidToPointItemMap.SetAsync(analogPoint.Gid, analogPoint); //seems redundant, but it sets in motion the update mechanism

                var result = await GidToPointItemMap.TryGetValueAsync(analogPoint.Gid);

                if (result.HasValue)
                {
                    return result.Value as IAnalogPointItem;
                }
                else
                {
                    throw new Exception("UpdateAnalogPointItemEguValue => TryGetValueAsync() returns no value");
                }
            }
            else if (GidToPointItemMap[gid] is IDiscretePointItem discretePoint)
            {
                discretePoint.CurrentValue = (ushort)rawValue;
                await GidToPointItemMap.SetAsync(discretePoint.Gid, discretePoint); //seems redundant, but it sets in motion the update mechanism

                var result = await GidToPointItemMap.TryGetValueAsync(discretePoint.Gid);

                if (result.HasValue)
                {
                    return result.Value as IDiscretePointItem;
                }
                else
                {
                    throw new Exception("UpdateAnalogPointItemEguValue => TryGetValueAsync() returns no value");
                }
            }
            else
            {
                string message = $"UpdatePointItemRawValue => Entity with Gid: 0x{gid:X16} does not implement IAnalogPointItem nor IDiscretePointItem.";
                Logger.LogError(message);
                throw new ArgumentException(message);
            }
        }

        public async Task AddOrUpdateCommandDescription(long gid, CommandDescription commandDescription)
        {
            while (!ReliableDictionariesInitialized || !(await GetIsScadaModelImportedIndicator()))
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            CommandDescriptionCache[gid] = commandDescription;
        }

        public async Task<bool> RemoveCommandDescription(long gid)
        {
            while (!ReliableDictionariesInitialized || !(await GetIsScadaModelImportedIndicator()))
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            return (await CommandDescriptionCache.TryRemoveAsync(gid)).HasValue;
        }
        #endregion IScadaModelUpdateAccessContract

        #region Private Methods
        private void PublishScadaData(Topic topic, ScadaMessage scadaMessage)
        {
            ScadaPublication scadaPublication = new ScadaPublication(topic, scadaMessage);
            this.publisherClient.Publish(scadaPublication);
            Logger.LogInformation($"SCADA service published data from topic: {scadaPublication.Topic}");

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("MeasurementCache content: ");

            foreach (long gid in MeasurementsCache.Keys)
            {
                IModbusData data = MeasurementsCache[gid];

                if (data is AnalogModbusData analogModbusData)
                {
                    sb.AppendLine($"Analog data line: [gid] 0x{gid:X16}, [value] {analogModbusData.Value}, [alarm] {analogModbusData.Alarm}");
                }
                else if (data is DiscreteModbusData discreteModbusData)
                {
                    sb.AppendLine($"Discrete data line: [gid] 0x{gid:X16}, [value] {discreteModbusData.Value}, [alarm] {discreteModbusData.Alarm}");
                }
                else
                {
                    sb.AppendLine($"UNKNOWN data type: {data.GetType()}");
                }
            }

            Logger.LogDebug(sb.ToString());
        }
        #endregion Private Methods
    }
}

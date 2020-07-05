using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Exceptions.SCADA;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.PubSub;
using OMS.Common.PubSubContracts;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using OMS.Common.ScadaContracts.DataContracts;
using OMS.Common.ScadaContracts.DataContracts.ScadaModelPointItems;
using OMS.Common.ScadaContracts.ModelProvider;
using OMS.Common.WcfClient.PubSub;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using ReliableDictionaryNames = OMS.Common.SCADA.ReliableDictionaryNames;

namespace SCADA.ModelProviderImplementation.ContractProviders
{
    public class ModelUpdateAccessProvider : IScadaModelUpdateAccessContract
    {
        private readonly string baseLogString;
        private readonly IReliableStateManager stateManager;

        private IPublisherContract publisherClient;

        #region Private Propetires
        private bool isGidToPointItemMapInitialized;
        private bool isMeasurementsCacheInitialized;
        private bool isCommandDescriptionCacheInitialized;
        private bool isInfoCacheInitialized;
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
            get { return gidToPointItemMap; }
        }

        private ReliableDictionaryAccess<long, ModbusData> measurementsCache;
        private ReliableDictionaryAccess<long, ModbusData> MeasurementsCache
        {
            get { return measurementsCache; }
        }

        private ReliableDictionaryAccess<long, CommandDescription> commandDescriptionCache;
        private ReliableDictionaryAccess<long, CommandDescription> CommandDescriptionCache
        {
            get { return commandDescriptionCache; }
        }

        private ReliableDictionaryAccess<string, bool> infoCache;
        private ReliableDictionaryAccess<string, bool> InfoCache
        {
            get { return infoCache; }
        }
        #endregion Private Propetires

        public ModelUpdateAccessProvider(IReliableStateManager stateManager)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

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

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.GidToPointItemMap}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.MeasurementsCache)
                {
                    //_ = MeasurementsCache;
                    measurementsCache = await ReliableDictionaryAccess<long, ModbusData>.Create(stateManager, ReliableDictionaryNames.MeasurementsCache);
                    this.isMeasurementsCacheInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.MeasurementsCache}' ReliableDictionaryAccess initialized.";
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
            string verboseMessage = $"{baseLogString} entering GetIsScadaModelImportedIndicator method.";
            Logger.LogVerbose(verboseMessage);

            while (!ReliableDictionariesInitialized)
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            string key = "IsScadaModelImported";
            if (!await InfoCache.ContainsKeyAsync(key))
            {
                await InfoCache.SetAsync(key, false);
            }

            bool isScadaModelImported = (await InfoCache.TryGetValueAsync(key)).Value;
            verboseMessage = $"{baseLogString} GetIsScadaModelImportedIndicator => returning value: {isScadaModelImported}.";
            Logger.LogVerbose(verboseMessage);

            return isScadaModelImported;
        }

        #region IScadaModelUpdateAccessContract
        public async Task MakeAnalogEntryToMeasurementCache(Dictionary<long, AnalogModbusData> data, bool permissionToPublishData)
        {
            string verboseMessage = $"{baseLogString} entering MakeAnalogEntryToMeasurementCache method.";
            Logger.LogVerbose(verboseMessage);

            while (!ReliableDictionariesInitialized || !(await GetIsScadaModelImportedIndicator()))
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            Dictionary<long, AnalogModbusData> publicationData = new Dictionary<long, AnalogModbusData>();

            if (data == null)
            {
                string message = $"{baseLogString} MakeAnalogEntryToMeasurementCache => data is null.";
                Logger.LogError(message);
                throw new NullReferenceException(message);
            }

            if (MeasurementsCache == null)
            {
                string message = $"{baseLogString} MakeAnalogEntryToMeasurementCache => gidToPointItemMap is null.";
                Logger.LogError(message);
                throw new InternalSCADAServiceException(message);
            }

            foreach (long gid in data.Keys)
            {
                if (!await MeasurementsCache.ContainsKeyAsync(gid))
                {
                    Logger.LogDebug($"{baseLogString} MakeAnalogEntryToMeasurementCache => Adding entry to MeasurementCache. Gid: {gid:X16}, Value: {data[gid].Value}, Alarm: {data[gid].Alarm}, CommandOrigin: {data[gid].CommandOrigin}");

                    await MeasurementsCache.SetAsync(gid, data[gid]);
                    publicationData[gid] = data[gid];
                }
                else
                {
                    var result = await MeasurementsCache.TryGetValueAsync(gid);
                    if(!result.HasValue)
                    {
                        string errorMessage = $"{baseLogString} MakeAnalogEntryToMeasurementCache => Gid 0x{gid:X16} does not exist in '{ReliableDictionaryNames.MeasurementsCache}'.";
                        Logger.LogError(errorMessage);
                        throw new Exception(errorMessage);
                    }

                    if(result.Value is AnalogModbusData analogCacheItem && analogCacheItem.Value != data[gid].Value)
                    {
                        Logger.LogDebug($"{baseLogString} MakeAnalogEntryToMeasurementCache => Value changed on element with Gid: 0x{analogCacheItem.MeasurementGid:X16}. Old value: {analogCacheItem.Value}, New value: {data[gid].Value}, Alarm: {data[gid].Alarm}, CommandOrigin: {data[gid].CommandOrigin}");

                        await MeasurementsCache.SetAsync(gid, data[gid]);
                        publicationData[gid] = data[gid];
                    }
                }
            }

            //if data is empty that means that there are no new values in the current acquisition cycle
            if (permissionToPublishData && publicationData.Count > 0)
            {
                ScadaMessage scadaMessage = new MultipleAnalogValueSCADAMessage(publicationData);
                await PublishScadaData(Topic.MEASUREMENT, scadaMessage);
            }
        }

        public async Task MakeDiscreteEntryToMeasurementCache(Dictionary<long, DiscreteModbusData> data, bool permissionToPublishData)
        {
            string verboseMessage = $"{baseLogString} entering MakeDiscreteEntryToMeasurementCache method.";
            Logger.LogVerbose(verboseMessage);

            while (!ReliableDictionariesInitialized || !(await GetIsScadaModelImportedIndicator()))
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            Dictionary<long, DiscreteModbusData> publicationData = new Dictionary<long, DiscreteModbusData>();

            if (data == null)
            {
                string message = $"{baseLogString} MakeDiscreteEntryToMeasurementCache => readAnalogCommand.Data is null.";
                Logger.LogError(message);
                throw new NullReferenceException(message);
            }

            if (MeasurementsCache == null)
            {
                string message = $"{baseLogString} MakeDiscreteEntryToMeasurementCache => gidToPointItemMap is null.";
                Logger.LogError(message);
                throw new InternalSCADAServiceException(message);
            }

            foreach (long gid in data.Keys)
            {
                if (!await MeasurementsCache.ContainsKeyAsync(gid))
                {
                    Logger.LogDebug($"{baseLogString} MakeDiscreteEntryToMeasurementCache => Adding entry to MeasurementCache. Gid: {gid:X16}, Value: {data[gid].Value}, Alarm: {data[gid].Alarm}, CommandOrigin: {data[gid].CommandOrigin}");

                    await MeasurementsCache.SetAsync(gid, data[gid]);
                    publicationData[gid] = data[gid];
                }
                else
                {
                    var result = await MeasurementsCache.TryGetValueAsync(gid);
                    if (!result.HasValue)
                    {
                        string errorMessage = $"{baseLogString} MakeAnalogEntryToMeasurementCache => Gid 0x{gid:X16} does not exist in '{ReliableDictionaryNames.MeasurementsCache}'.";
                        Logger.LogError(errorMessage);
                        throw new Exception(errorMessage);
                    }

                    if(result.Value is DiscreteModbusData discreteCacheItem && discreteCacheItem.Value != data[gid].Value)
                    {
                        Logger.LogDebug($"{baseLogString} MakeDiscreteEntryToMeasurementCache => Value changed on element with Gid: 0x{discreteCacheItem.MeasurementGid:X16}; Old value: {discreteCacheItem.Value}; new value: {data[gid].Value}");

                        await MeasurementsCache.SetAsync(gid, data[gid]);
                        publicationData[gid] = data[gid];
                    }
                }
            }

            //if data is empty that means that there are no new values in the current acquisition cycle
            if (permissionToPublishData && publicationData.Count > 0)
            {
                ScadaMessage scadaMessage = new MultipleDiscreteValueSCADAMessage(publicationData);
                await PublishScadaData(Topic.SWITCH_STATUS, scadaMessage);
            }
        }

        public async Task<IScadaModelPointItem> UpdatePointItemRawValue(long gid, int rawValue)
        {
            string verboseMessage = $"{baseLogString} entering UpdatePointItemRawValue method.";
            Logger.LogVerbose(verboseMessage);

            while (!ReliableDictionariesInitialized || !(await GetIsScadaModelImportedIndicator()))
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            if (!await GidToPointItemMap.ContainsKeyAsync(gid))
            {
                string message = $"{baseLogString} UpdatePointItemRawValue => Entity with Gid: 0x{gid:X16} does not exist in GidToPointItemMap.";
                Logger.LogError(message);
                throw new ArgumentException(message);
            }

            if ((await GidToPointItemMap.TryGetValueAsync(gid)).Value is IAnalogPointItem analogPoint)
            {
                if (!analogPoint.Initialized)
                {
                    string errorMessage = $"{baseLogString} SendSingleAnalogCommand => PointItem was initialized. Gid: 0x{analogPoint.Gid:X16}, Addres: {analogPoint.Address}, Name: {analogPoint.Name}, RegisterType: {analogPoint.RegisterType}, Initialized: {analogPoint.Initialized}";
                    Logger.LogError(errorMessage);
                }

                analogPoint.CurrentEguValue = analogPoint.RawToEguValueConversion(rawValue);
                await GidToPointItemMap.SetAsync(analogPoint.Gid, analogPoint);

                var result = await GidToPointItemMap.TryGetValueAsync(analogPoint.Gid);

                if (result.HasValue)
                {
                    return result.Value as IAnalogPointItem;
                }
                else
                {
                    string errorMessage = $"{baseLogString} UpdateAnalogPointItemEguValue => TryGetValueAsync() returns no value";
                    Logger.LogError(errorMessage);
                    throw new Exception(errorMessage);
                }
            }
            else if ((await GidToPointItemMap.TryGetValueAsync(gid)).Value is IDiscretePointItem discretePoint)
            {
                discretePoint.CurrentValue = (ushort)rawValue;
                await GidToPointItemMap.SetAsync(discretePoint.Gid, discretePoint);

                var result = await GidToPointItemMap.TryGetValueAsync(discretePoint.Gid);

                if (result.HasValue)
                {
                    return result.Value as IDiscretePointItem;
                }
                else
                {
                    string errorMessage = $"{baseLogString} UpdateAnalogPointItemEguValue => TryGetValueAsync() returns no value.";
                    Logger.LogError(errorMessage);
                    throw new Exception(errorMessage);
                }
            }
            else
            {
                string errorMessage = $"{baseLogString} UpdatePointItemRawValue => Entity with Gid: 0x{gid:X16} does not implement IAnalogPointItem nor IDiscretePointItem.";
                Logger.LogError(errorMessage);
                throw new ArgumentException(errorMessage);
            }
        }

        public async Task AddOrUpdateCommandDescription(long gid, CommandDescription commandDescription)
        {
            string verboseMessage = $"{baseLogString} entering AddOrUpdateCommandDescription method.";
            Logger.LogVerbose(verboseMessage);

            while (!ReliableDictionariesInitialized || !(await GetIsScadaModelImportedIndicator()))
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            await CommandDescriptionCache.SetAsync(gid, commandDescription);
        }

        public async Task<bool> RemoveCommandDescription(long gid)
        {
            string verboseMessage = $"{baseLogString} entering RemoveCommandDescription method.";
            Logger.LogVerbose(verboseMessage);

            while (!ReliableDictionariesInitialized || !(await GetIsScadaModelImportedIndicator()))
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            return (await CommandDescriptionCache.TryRemoveAsync(gid)).HasValue;
        }
        #endregion IScadaModelUpdateAccessContract

        #region Private Methods
        private async Task PublishScadaData(Topic topic, ScadaMessage scadaMessage)
        {
            string verboseMessage = $"{baseLogString} entering PublishScadaData method.";
            Logger.LogVerbose(verboseMessage);

            try
            {
                ScadaPublication scadaPublication = new ScadaPublication(topic, scadaMessage);
                await this.publisherClient.Publish(scadaPublication);
                Logger.LogInformation($"{baseLogString} PublishScadaData => SCADA service published data of topic: {scadaPublication.Topic}");
            }
            catch (CommunicationObjectFaultedException e)
            {
                string message = $"{baseLogString} PublishScadaData => CommunicationObjectFaultedException caught.";
                Logger.LogError(message, e);

                await Task.Delay(2000);

                this.publisherClient = PublisherClient.CreateClient();
                await PublishScadaData(topic, scadaMessage);
                //todo: different logic on multiple rety?
            }

            //StringBuilder sb = new StringBuilder();
            //sb.AppendLine($"{baseLogString} PublishScadaData => MeasurementCache content: ");

            //foreach (long gid in MeasurementsCache.Keys)
            //{
            //    IModbusData data = MeasurementsCache.TryGetValueAsync();

            //    if (data is AnalogModbusData analogModbusData)
            //    {
            //        sb.AppendLine($"Analog data line: [gid] 0x{gid:X16}, [value] {analogModbusData.Value}, [alarm] {analogModbusData.Alarm}");
            //    }
            //    else if (data is DiscreteModbusData discreteModbusData)
            //    {
            //        sb.AppendLine($"Discrete data line: [gid] 0x{gid:X16}, [value] {discreteModbusData.Value}, [alarm] {discreteModbusData.Alarm}");
            //    }
            //    else
            //    {
            //        sb.AppendLine($"UNKNOWN data type: {data.GetType()}");
            //    }
            //}

            //Logger.LogDebug(sb.ToString());
        }
        #endregion Private Methods
    }
}

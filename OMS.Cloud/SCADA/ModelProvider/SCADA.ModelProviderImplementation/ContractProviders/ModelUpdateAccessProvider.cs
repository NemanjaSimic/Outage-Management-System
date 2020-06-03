using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts.DataContracts;
using OMS.Common.ScadaContracts.ModelProvider;
using Outage.Common;
using Outage.Common.Exceptions.SCADA;
using Outage.Common.PubSub.SCADADataContract;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace SCADA.ModelProviderImplementation.ContractProviders
{
    //[ServiceBehavior(AddressFilterMode = AddressFilterMode.Any)]
    public class ModelUpdateAccessProvider : IScadaModelUpdateAccessContract
    {
        private readonly IReliableStateManager stateManager;
        private bool isMeasurementsCacheInitialized;
        private bool isCommandDescriptionCacheInitialized;
        private bool isInfoCacheInitialized;

        #region Private Propetires
        private bool ReliableDictionariesInitialized
        {
            get { return isMeasurementsCacheInitialized && isCommandDescriptionCacheInitialized && isInfoCacheInitialized; }
        }

        private ILogger logger;
        private ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
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

            this.isMeasurementsCacheInitialized = false;
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

                if (reliableStateName == ReliableDictionaryNames.MeasurementsCache)
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
                    await MeasurementsCache.Add(gid, data[gid]);
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
                SCADAMessage scadaMessage = new MultipleAnalogValueSCADAMessage(publicationData);
                //TODO: PublishScadaData(Topic.MEASUREMENT, scadaMessage);
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
                    await MeasurementsCache.Add(gid, data[gid]);
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
                SCADAMessage scadaMessage = new MultipleDiscreteValueSCADAMessage(publicationData);
                //TODO: PublishScadaData(Topic.SWITCH_STATUS, scadaMessage);
            }
        }

        public async Task UpdateCommandDescription(long gid, CommandDescription commandDescription)
        {
            while (!ReliableDictionariesInitialized || !(await GetIsScadaModelImportedIndicator()))
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            CommandDescriptionCache[gid] = commandDescription;
        }
        #endregion IScadaModelUpdateAccessContract

        //TODO: publish...
        //private void PublishScadaData(Topic topic, SCADAMessage scadaMessage)
        //{
        //    SCADAPublication scadaPublication = new SCADAPublication(topic, scadaMessage);

        //    using (PublisherProxy publisherProxy = proxyFactory.CreateProxy<PublisherProxy, IPublisher>(EndpointNames.PublisherEndpoint))
        //    {
        //        if (publisherProxy == null)
        //        {
        //            string errMsg = "PublisherProxy is null.";
        //            Logger.LogWarn(errMsg);
        //            throw new NullReferenceException(errMsg);
        //        }

        //        publisherProxy.Publish(scadaPublication, "SCADA_PUBLISHER");
        //        Logger.LogInfo($"SCADA service published data from topic: {scadaPublication.Topic}");

        //        StringBuilder sb = new StringBuilder();
        //        sb.AppendLine("MeasurementCache content: ");

        //        foreach (long gid in MeasurementsCache.Keys)
        //        {
        //            IModbusData data = MeasurementsCache[gid];

        //            if (data is AnalogModbusData analogModbusData)
        //            {
        //                sb.AppendLine($"Analog data line: [gid] 0x{gid:X16}, [value] {analogModbusData.Value}, [alarm] {analogModbusData.Alarm}");
        //            }
        //            else if (data is DiscreteModbusData discreteModbusData)
        //            {
        //                sb.AppendLine($"Discrete data line: [gid] 0x{gid:X16}, [value] {discreteModbusData.Value}, [alarm] {discreteModbusData.Alarm}");
        //            }
        //            else
        //            {
        //                sb.AppendLine($"UNKNOWN data type: {data.GetType()}");
        //            }
        //        }

        //        Logger.LogDebug(sb.ToString());
        //    }
        //}
    }
}

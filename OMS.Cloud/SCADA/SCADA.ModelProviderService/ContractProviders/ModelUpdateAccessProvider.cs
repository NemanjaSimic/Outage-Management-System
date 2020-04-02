using OMS.Common.ScadaContracts;
using Outage.Common;
using Outage.Common.PubSub.SCADADataContract;
using System;
using System.Collections.Generic;

namespace OMS.Cloud.SCADA.ModelProviderService.ContractProviders
{
    internal class ModelUpdateAccessProvider : IScadaModelUpdateAccessContract
    {
        private ILogger logger;
        private ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        public void MakeAnalogEntryToMeasurementCache(Dictionary<long, AnalogModbusData> data, bool permissionToPublishData)
        {
            Dictionary<long, AnalogModbusData> publicationData = new Dictionary<long, AnalogModbusData>();

            if (data == null)
            {
                string message = $"WriteToMeasurementsCache() => readAnalogCommand.Data is null.";
                Logger.LogError(message);
                throw new NullReferenceException(message);
            }

            //TODO: get MeasurementsCache...

            foreach (long gid in data.Keys)
            {
                if (!MeasurementsCache.ContainsKey(gid))
                {
                    MeasurementsCache.Add(gid, data[gid]);
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

        public void MakeDiscreteEntryToMeasurementCache(Dictionary<long, DiscreteModbusData> data, bool permissionToPublishData)
        {
            Dictionary<long, DiscreteModbusData> publicationData = new Dictionary<long, DiscreteModbusData>();

            if (data == null)
            {
                string message = $"WriteToMeasurementsCache() => readAnalogCommand.Data is null.";
                Logger.LogError(message);
                throw new NullReferenceException(message);
            }

            //TODO: get MeasurementsCache...

            foreach (long gid in data.Keys)
            {
                if (!MeasurementsCache.ContainsKey(gid))
                {
                    MeasurementsCache.Add(gid, data[gid]);
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

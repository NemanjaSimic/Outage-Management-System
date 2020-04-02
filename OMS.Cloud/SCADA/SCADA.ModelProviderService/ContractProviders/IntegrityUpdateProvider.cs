using OMS.Cloud.SCADA.Data.Repository;
using OMS.Common.ScadaContracts;
using Outage.Common;
using Outage.Common.Exceptions.SCADA;
using Outage.Common.PubSub.SCADADataContract;
using System;
using System.Collections.Generic;

namespace OMS.Cloud.SCADA.ModelProviderService.ContractProviders
{
    internal class IntegrityUpdateProvider : IScadaIntegrityUpdateContract
    {
        private ILogger logger;
        private ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        #region IScadaIntegrityUpdateContract
        public Dictionary<Topic, SCADAPublication> GetIntegrityUpdate()
        {
            if (scadaModel == null)
            {
                string message = $"GetIntegrityUpdate => SCADA model is null.";
                Logger.LogError(message);
                throw new InternalSCADAServiceException(message);
            }

            var currentScadaModel = scadaModel.CurrentScadaModel;
            var commandValuesCache = scadaModel.CommandedValuesCache;

            Dictionary<long, AnalogModbusData> analogModbusData = new Dictionary<long, AnalogModbusData>();
            Dictionary<long, DiscreteModbusData> discreteModbusData = new Dictionary<long, DiscreteModbusData>();

            foreach (long gid in currentScadaModel.Keys)
            {
                CommandOriginType commandOrigin = CommandOriginType.OTHER_COMMAND;

                if (currentScadaModel[gid] is AnalogSCADAModelPointItem analogPointItem)
                {
                    if (commandValuesCache.ContainsKey(gid) && commandValuesCache[gid].Value == analogPointItem.CurrentRawValue)
                    {
                        commandOrigin = commandValuesCache[gid].CommandOrigin;
                    }

                    AnalogModbusData analogValue = new AnalogModbusData(analogPointItem.CurrentEguValue, analogPointItem.Alarm, gid, commandOrigin);
                    analogModbusData.Add(gid, analogValue);
                }
                else if (currentScadaModel[gid] is DiscreteSCADAModelPointItem discretePointItem)
                {
                    if (commandValuesCache.ContainsKey(gid) && commandValuesCache[gid].Value == discretePointItem.CurrentValue)
                    {
                        commandOrigin = commandValuesCache[gid].CommandOrigin;
                    }

                    DiscreteModbusData discreteValue = new DiscreteModbusData(discretePointItem.CurrentValue, discretePointItem.Alarm, gid, commandOrigin);
                    discreteModbusData.Add(gid, discreteValue);
                }
            }

            MultipleAnalogValueSCADAMessage analogValuesMessage = new MultipleAnalogValueSCADAMessage(analogModbusData);
            SCADAPublication measurementPublication = new SCADAPublication(Topic.MEASUREMENT, analogValuesMessage);

            MultipleDiscreteValueSCADAMessage discreteValuesMessage = new MultipleDiscreteValueSCADAMessage(discreteModbusData);
            SCADAPublication switchStatusPublication = new SCADAPublication(Topic.SWITCH_STATUS, discreteValuesMessage);

            Dictionary<Topic, SCADAPublication> scadaPublications = new Dictionary<Topic, SCADAPublication>
            {
                { Topic.MEASUREMENT, measurementPublication },
                { Topic.SWITCH_STATUS, switchStatusPublication },
            };

            return scadaPublications;
        }

        public SCADAPublication GetIntegrityUpdateForSpecificTopic(Topic topic)
        {
            if (scadaModel == null)
            {
                string message = $"GetIntegrityUpdate => SCADA model is null.";
                Logger.LogError(message);
                throw new InternalSCADAServiceException(message);
            }

            SCADAPublication scadaPublication;

            var currentScadaModel = scadaModel.CurrentScadaModel;
            var commandValuesCache = scadaModel.CommandedValuesCache;

            Dictionary<long, AnalogModbusData> analogModbusData = new Dictionary<long, AnalogModbusData>();
            Dictionary<long, DiscreteModbusData> discreteModbusData = new Dictionary<long, DiscreteModbusData>();

            foreach (long gid in currentScadaModel.Keys)
            {
                CommandOriginType commandOrigin = CommandOriginType.OTHER_COMMAND;

                if (topic == Topic.MEASUREMENT && currentScadaModel[gid] is AnalogSCADAModelPointItem analogPointItem)
                {
                    if (commandValuesCache.ContainsKey(gid) && commandValuesCache[gid].Value == analogPointItem.CurrentRawValue)
                    {
                        commandOrigin = commandValuesCache[gid].CommandOrigin;
                    }

                    AnalogModbusData analogValue = new AnalogModbusData(analogPointItem.CurrentEguValue, analogPointItem.Alarm, gid, commandOrigin);
                    analogModbusData.Add(gid, analogValue);
                }
                else if (topic == Topic.SWITCH_STATUS && currentScadaModel[gid] is DiscreteSCADAModelPointItem discretePointItem)
                {
                    if (commandValuesCache.ContainsKey(gid) && commandValuesCache[gid].Value == discretePointItem.CurrentValue)
                    {
                        commandOrigin = commandValuesCache[gid].CommandOrigin;
                    }

                    DiscreteModbusData discreteValue = new DiscreteModbusData(discretePointItem.CurrentValue, discretePointItem.Alarm, gid, commandOrigin);
                    discreteModbusData.Add(gid, discreteValue);
                }
            }

            if (topic == Topic.MEASUREMENT)
            {
                MultipleAnalogValueSCADAMessage analogValuesMessage = new MultipleAnalogValueSCADAMessage(analogModbusData);
                scadaPublication = new SCADAPublication(Topic.MEASUREMENT, analogValuesMessage);

            }
            else if (topic == Topic.SWITCH_STATUS)
            {
                MultipleDiscreteValueSCADAMessage discreteValuesMessage = new MultipleDiscreteValueSCADAMessage(discreteModbusData);
                scadaPublication = new SCADAPublication(Topic.SWITCH_STATUS, discreteValuesMessage);
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

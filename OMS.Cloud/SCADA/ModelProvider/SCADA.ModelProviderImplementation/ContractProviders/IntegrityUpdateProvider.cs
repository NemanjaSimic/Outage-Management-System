using Common.SCADA;
using Microsoft.ServiceFabric.Data;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts;
using OMS.Common.ScadaContracts.DataContracts;
using Outage.Common;
using Outage.Common.Exceptions.SCADA;
using Outage.Common.PubSub.SCADADataContract;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SCADA.ModelProviderImplementation.ContractProviders
{
    public class IntegrityUpdateProvider : IScadaIntegrityUpdateContract
    {
        private readonly IReliableStateManager stateManager;

        private ILogger logger;
        private ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        private ReliableDictionaryAccess<long, IScadaModelPointItem> gidToPointItemMap;
        private ReliableDictionaryAccess<long, IScadaModelPointItem> GidToPointItemMap
        {
            get
            {
                return gidToPointItemMap ?? (gidToPointItemMap = new ReliableDictionaryAccess<long, IScadaModelPointItem>(stateManager, ReliableDictionaryNames.GidToPointItemMap));
            }
        }

        private ReliableDictionaryAccess<long, CommandDescription> commandDescriptionCache;
        private ReliableDictionaryAccess<long, CommandDescription> CommandDescriptionCache
        {
            get
            {
                return commandDescriptionCache ?? (commandDescriptionCache = new ReliableDictionaryAccess<long, CommandDescription>(stateManager, ReliableDictionaryNames.CommandDescriptionCache));
            }
        }

        public IntegrityUpdateProvider(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
        }

        #region IScadaIntegrityUpdateContract
        public async Task<Dictionary<Topic, SCADAPublication>> GetIntegrityUpdate()
        {
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

        public async Task<SCADAPublication> GetIntegrityUpdateForSpecificTopic(Topic topic)
        {
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

            SCADAPublication scadaPublication;

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

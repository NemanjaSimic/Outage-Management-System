using Common.PubSubContracts.DataContracts.CE;
using Common.PubSubContracts.DataContracts.OMS;
using Common.Web.Mappers;
using Common.Web.Models.ViewModels;
using OMS.Common.Cloud.Logger;
using OMS.Common.PubSubContracts;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using OMS.Common.PubSubContracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebAdapterImplementation.HubDispatchers;

namespace WebAdapterImplementation
{
    public class NotifySubscriberProvider : INotifySubscriberContract
    {
        private readonly string baseLogString;
        private readonly string _subscriberName;
        private readonly IGraphMapper _graphMapper;
        private readonly IOutageMapper _outageMapper;

        private ICloudLogger logger;
        protected ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        public NotifySubscriberProvider(string subscriberName)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");

            _subscriberName = subscriberName;
            _graphMapper = new GraphMapper();
            _outageMapper = new OutageMapper(new ConsumerMapper(), new EquipmentMapper());
        }

        public async Task Notify(IPublishableMessage message, string publisherName)
        {
            if (message is ActiveOutageMessage activeOutage)
            {
                var outageDispatcher = new OutageHubDispatcher(_outageMapper);
                outageDispatcher.Connect();

                try
                {
                    await outageDispatcher.NotifyActiveOutageUpdate(activeOutage);
                }
                catch (Exception e)
                {
                    string errorMessage = $"{baseLogString} Notify => exception {e.Message}";
                    Logger.LogError(errorMessage, e);
                }
            }
            else if (message is ArchivedOutageMessage archivedOutage)
            {
                var outageDispatcher = new OutageHubDispatcher(_outageMapper);
                outageDispatcher.Connect();

                try
                {
                    await outageDispatcher.NotifyArchiveOutageUpdate(archivedOutage);
                }
                catch (Exception e)
                {
                    string errorMessage = $"{baseLogString} Notify => exception {e.Message}";
                    Logger.LogError(errorMessage, e);
                }
            }
            else if (message is TopologyForUIMessage topologyMessage)
            {
                OmsGraphViewModel graph = _graphMapper.Map(topologyMessage.UIModel);

                var graphDispatcher = new GraphHubDispatcher();
                graphDispatcher.Connect();

                try
                {
                    await graphDispatcher.NotifyGraphUpdate(graph.Nodes, graph.Relations);
                }
                catch (Exception e)
                {
                    string errorMessage = $"{baseLogString} Notify => exception {e.Message}";
                    Logger.LogError(errorMessage, e);
                }

            } 
            else if (message is MultipleAnalogValueSCADAMessage analogValuesMessage)
            {
                Dictionary<long, AnalogModbusData> analogModbusData = new Dictionary<long, AnalogModbusData>(analogValuesMessage.Data);
                
                var scadaDispatcher = new ScadaHubDispatcher();
                scadaDispatcher.Connect();

                try
                {
                    await scadaDispatcher.NotifyScadaDataUpdate(analogModbusData);
                }
                catch (Exception e)
                {
                    string errorMessage = $"{baseLogString} Notify => exception {e.Message}";
                    Logger.LogError(errorMessage, e);
                }
            }
        }

        public Task<string> GetSubscriberName()
        {
            return Task.Run(() => _subscriberName);
        }

        public Task<bool> IsAlive()
        {
            return Task.Run(() => true);
        }
    }
}

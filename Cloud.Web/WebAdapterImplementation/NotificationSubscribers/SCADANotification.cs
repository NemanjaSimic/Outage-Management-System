using Common.PubSub;
using OMS.Common.Cloud.Logger;
using OMS.Common.PubSubContracts;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading.Tasks;
using WebAdapterImplementation.HubDispatchers;

namespace WebAdapterImplementation.NotificationSubscribers
{
    [DataContract]
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    class SCADANotification : INotifySubscriberContract
    {
        private readonly string baseLogString;

        private ICloudLogger logger;
        protected ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        private readonly string _subscriberName;
        private ScadaHubDispatcher _dispatcher;

        public SCADANotification(string subscriberName)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
            Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");

            _subscriberName = subscriberName;
        }


        public async Task Notify(IPublishableMessage message, string publisherName)
        {
            if (message is MultipleAnalogValueSCADAMessage analogValuesMessage)
            {
                Dictionary<long, AnalogModbusData> analogModbusData = new Dictionary<long, AnalogModbusData>(analogValuesMessage.Data);
                _dispatcher = new ScadaHubDispatcher();
                _dispatcher.Connect();

                try
                {
                    _dispatcher.NotifyScadaDataUpdate(analogModbusData);
                }
                catch (Exception e)
                {
                    string errorMessage = $"{baseLogString} Notify => exception {e.Message}";
                    Logger.LogError(errorMessage, e);
                }
            }
        }

        public async Task<string> GetSubscriberName()
        {
            return _subscriberName;
        }
    }
}

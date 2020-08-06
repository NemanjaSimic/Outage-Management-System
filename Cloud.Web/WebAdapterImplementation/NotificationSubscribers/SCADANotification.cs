using Common.PubSub;
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
        private readonly string _subscriberName;
        private ScadaHubDispatcher _dispatcher;

        public SCADANotification(string subscriberName)
        {
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
                catch (Exception)
                {
                    // TODO: log error
                }
            }
        }

        public async Task<string> GetSubscriberName()
        {
            return _subscriberName;
        }
    }
}

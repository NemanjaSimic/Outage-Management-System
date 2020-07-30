using Outage.Common.PubSub;
using Outage.Common.PubSub.SCADADataContract;
using Outage.Common.ServiceContracts.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using WebAdapter.HubDispatchers;

namespace WebAdapter.Scada
{
    [DataContract]
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    class SCADANotification : ISubscriberCallback
    {
        private readonly string _subscriberName;
        private readonly ScadaHubDispatcher _dispatcher;

        public SCADANotification(string subscriberName)
        {
            _subscriberName = subscriberName;

            _dispatcher = new ScadaHubDispatcher();
        }

        public string GetSubscriberName() => _subscriberName;

        public void Notify(IPublishableMessage message)
        {

            if (message is MultipleAnalogValueSCADAMessage analogValuesMessage)
            {
                Dictionary<long, AnalogModbusData> analogModbusData = new Dictionary<long, AnalogModbusData>(analogValuesMessage.Data);

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
    }
}

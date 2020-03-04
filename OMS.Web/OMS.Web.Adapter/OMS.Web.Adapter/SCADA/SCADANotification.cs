using OMS.Web.Adapter.HubDispatchers;
using Outage.Common.PubSub;
using Outage.Common.PubSub.SCADADataContract;
using Outage.Common.ServiceContracts.PubSub;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace OMS.Web.Adapter.SCADA
{
    [DataContract]
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class SCADANotification : ISubscriberCallback
    {
        private readonly string _subscriberName;
        private readonly ScadaHubDipatcher _dispatcher;

        public SCADANotification(string subscriberName)
        {
            _subscriberName = subscriberName;

            _dispatcher = new ScadaHubDipatcher();
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
                    //log error
                    // retry ?
                }
            }
            else
            {
                //todo: if anything?
            }
        }
    }
}

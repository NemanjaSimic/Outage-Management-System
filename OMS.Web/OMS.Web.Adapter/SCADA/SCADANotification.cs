using Outage.Common.PubSub;
using Outage.Common.PubSub.CalculationEngineDataContract;
using Outage.Common.ServiceContracts.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Web.Adapter.SCADA
{
    [DataContract]
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class SCADANotification : ISubscriberCallback
    {
        private readonly string _subscriberName;

        public SCADANotification(string subscriberName)
        {
            _subscriberName = subscriberName;
        }

        public string GetSubscriberName() => _subscriberName;

        public void Notify(IPublishableMessage message)
        {
            if (message is MultipleAnalogScadaMessage topologyMessage)
            {

            }
        }
}

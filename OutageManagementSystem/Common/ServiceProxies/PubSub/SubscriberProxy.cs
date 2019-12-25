using Outage.Common.ServiceContracts.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.ServiceProxies.PubSub
{
    public class SubscriberProxy : DuplexClientBase<ISubscriber>, ISubscriber
    {
        public SubscriberProxy(IPubSubNotification callbackInstance, string endpointName) 
            : base(callbackInstance, endpointName)
        {
        }

        public void Subscribe(Topic topic)
        {
            try
            {
                Channel.Subscribe(topic);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

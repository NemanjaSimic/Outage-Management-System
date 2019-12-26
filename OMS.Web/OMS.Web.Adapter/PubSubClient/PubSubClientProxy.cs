using Outage.Common.ServiceProxies.PubSub;

namespace OMS.Web.Adapter.PubSubClient
{
    public class PubSubClientProxy : SubscriberProxy
    {
        public PubSubClientProxy(string endpointName)
            : base(new PubSubNotification(), endpointName) { }
    }
}

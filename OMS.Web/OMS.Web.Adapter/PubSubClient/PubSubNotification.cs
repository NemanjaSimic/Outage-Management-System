using System;
using Outage.Common.PubSub;
using Outage.Common.ServiceContracts.PubSub;

namespace OMS.Web.Adapter.PubSubClient
{
    public class PubSubNotification : IPubSubNotification
    {
        public void Notify(IPublishableMessage message)
        {
            Console.WriteLine($"Message from PubSub: {message}");
        }
    }
}

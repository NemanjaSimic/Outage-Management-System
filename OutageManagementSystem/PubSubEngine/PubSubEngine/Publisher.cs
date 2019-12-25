using Outage.Common.PubSub;
using Outage.Common.ServiceContracts.PubSub;
using System.Collections.Generic;
using System.ServiceModel;

namespace PubSubEngine
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    internal class Publisher : IPublisher
    {
        public void Publish(IPublication publication)
        {
            List<IPubSubNotification> listOfSubscribers = Publications.Instance.GetAllSubscribers(publication.Topic);

            if (listOfSubscribers != null)
            {
                foreach (var item in listOfSubscribers)
                {
                    Subscribers.Instance.PublishMessage(item, publication.Message);
                }
            }
        }
    }
}
using Outage.Common;
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
            ILogger logger = LoggerWrapper.Instance;

            List<ISubscriberCallback> listOfSubscribers = Publications.Instance.GetAllSubscribers(publication.Topic);

            if (listOfSubscribers != null)
            {
                foreach (ISubscriberCallback subscriber in listOfSubscribers)
                {
                    Subscribers.Instance.PublishMessage(subscriber, publication.Message);
                    logger.LogInfo($"Publication [Topic: {publication.Topic}] SUCCESSFULLY published to Subscriber [{subscriber.SubscriberName}]");
                }
            }
        }
    }
}
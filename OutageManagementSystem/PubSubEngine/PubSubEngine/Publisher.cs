using Outage.Common;
using Outage.Common.PubSub;
using Outage.Common.ServiceContracts.PubSub;
using System;
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

            // Konstruktor kopije kako ne bi pucao exception u foreachu ako se originalna lista subscribera izmeni u sred petlje
            List<ISubscriberCallback> listOfSubscribers = new List<ISubscriberCallback>(Publications.Instance.GetAllSubscribers(publication.Topic));

            if (listOfSubscribers != null)
            {
                foreach (ISubscriberCallback subscriber in listOfSubscribers)
                {
                    try
                    {
                        string subscriberName = subscriber.GetSubscriberName();
                        Subscribers.Instance.PublishMessage(subscriber, publication.Message);
                        logger.LogInfo($"Publication [Topic: {publication.Topic}] SUCCESSFULLY published to Subscriber [{subscriberName}]");
                    }
                    catch (Exception e)
                    {
                        Subscribers.Instance.RemoveSubscriber(subscriber);
                        Publications.Instance.RemoveSubscriber(subscriber);
                        logger.LogWarn($"Failed to publish. Subscriber is no longer in subscriber list.");
                    }             
                }
            }
        }
    }
}
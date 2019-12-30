using Outage.Common;
using Outage.Common.PubSub;
using Outage.Common.ServiceContracts.PubSub;
using System;
using System.ServiceModel;
using System.Threading;

namespace PubSubEngine
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
    internal class Subscriber : ISubscriber
    {
        private static ILogger logger = LoggerWrapper.Instance;

        public void Subscribe(Topic topic)
        {
            ISubscriberCallback subscriber = OperationContext.Current.GetCallbackChannel<ISubscriberCallback>();

            if(Subscribers.Instance.TryAddSubscriber(subscriber) && Publications.Instance.TryAddSubscriber(topic, subscriber))
            {
                logger.LogInfo($"Subscriber [{subscriber.SubscriberName}, Topic: {topic}] SUCCESSFYLLY.");
                Thread thread = new Thread(() => TrackPublications(subscriber));
                thread.Start();
            }
            else
            {
                string message = $"Try to add Subscriber [{subscriber.SubscriberName}, Topic: {topic}] FAILED.";
                logger.LogError(message);
                throw new Exception(message);
            }
        }

        private void TrackPublications(ISubscriberCallback subscriber)
        {
            bool end = false;

            logger.LogInfo($"Thread for tracking publications STARTED. Subscriber [{subscriber.SubscriberName}]");

            while (!end)
            {
                IPublishableMessage message = Subscribers.Instance.GetNextMessage(subscriber);

                if (message != null)
                {
                    try
                    {
                        subscriber.Notify(message);
                        logger.LogDebug($"Subscriber [{subscriber.SubscriberName}] notified SUCCESSFULLY.");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Exception on notifying Subscriber [{subscriber.SubscriberName}].", ex);

                        Subscribers.Instance.RemoveSubscriber(subscriber);
                        Publications.Instance.RemoveSubscriber(subscriber);
                        end = true;
                    }
                }

                Thread.Sleep(200);
            }

            logger.LogInfo($"Thread for tracking publications STOPPED. Subscriber [{subscriber.SubscriberName}]");
        }
    }
}
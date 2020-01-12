using Outage.Common;
using Outage.Common.PubSub;
using Outage.Common.ServiceContracts.PubSub;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PubSubEngine
{
    public class Subscribers
    {
        private static ILogger Logger = LoggerWrapper.Instance;
        private ConcurrentDictionary<ISubscriberCallback, Queue<IPublishableMessage>> subscribers;

        private static Subscribers instance;

        public static Subscribers Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Subscribers();
                }
                return instance;
            }
        }

        private Subscribers()
        {
            subscribers = new ConcurrentDictionary<ISubscriberCallback, Queue<IPublishableMessage>>();
        }

        public bool TryAddSubscriber(ISubscriberCallback subscriber)
        {
            bool success = subscribers.TryAdd(subscriber, new Queue<IPublishableMessage>());
            string subscriberName = subscriber.GetSubscriberName();

            if (success)
            {
                Logger.LogDebug($"Subscriber [{subscriberName}] SUCCESSFYLLY added to collection of all subscribers.");
            }
            else
            {
                Logger.LogWarn($"Try to add Subscriber [{subscriberName}] FAILED.");
            }

            return success;
        }

        public void RemoveSubscriber(ISubscriberCallback subscriber)
        {
            bool success = subscribers.TryRemove(subscriber, out Queue<IPublishableMessage> queue);
            string subscriberName = subscriber.GetSubscriberName();

            if (success)
            {
                Logger.LogDebug($"Subscriber [{subscriberName}] SUCCESSFYLLY removed from collection of all subscribers.");
            }
            else
            {
                Logger.LogWarn($"Try to remove Subscriber [{subscriberName}] FAILED.");
            }
        }

        public void PublishMessage(ISubscriberCallback subscriber, IPublishableMessage message)
        {
            bool success = subscribers.TryGetValue(subscriber, out Queue<IPublishableMessage> queueOfMessages);
            string subscriberName = subscriber.GetSubscriberName();

            if (success)
            {
                queueOfMessages.Enqueue(message);
                //TODO: check this log in particular
                Logger.LogDebug($"Published message [{message}] SUCCESSFYLLY enqueued on Subscriber [{subscriberName}]");
            }
            else
            {
                Logger.LogWarn($"Try to get queue of messages for Subscriber [{subscriberName}] FAILED.");
            }
        }

        public IPublishableMessage GetNextMessage(ISubscriberCallback subscriber)
        {
            IPublishableMessage message = null;

            bool success = subscribers.TryGetValue(subscriber, out Queue<IPublishableMessage> queueOfMessages) && queueOfMessages.Count > 0;
            string subscriberName = subscriber.GetSubscriberName();

            if (success)
            {
                message = queueOfMessages.Dequeue();
                Logger.LogDebug($"Published message [{message}] SUCCESSFYLLY dequeued from Subscriber's queue of messages [Subscriber name: '{subscriberName}']");
            }
            else
            {
                Logger.LogWarn($"Try to get queue of messages for Subscriber [{subscriberName}] FAILED or queue is empty.");
            }

            return message;
        }
    }
}
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

            if (success)
            {
                Logger.LogDebug($"Subscriber [{subscriber.SubscriberName}] SUCCESSFYLLY added to collection of all subscribers.");
            }
            else
            {
                Logger.LogError($"Try to add Subscriber [{subscriber.SubscriberName}] FAILED.");
            }

            return success;
        }

        public void RemoveSubscriber(ISubscriberCallback subscriber)
        {
            bool success = subscribers.TryRemove(subscriber, out Queue<IPublishableMessage> queue);

            if (success)
            {
                Logger.LogDebug($"Subscriber [{subscriber.SubscriberName}] SUCCESSFYLLY removed from collection of all subscribers.");
            }
            else
            {
                Logger.LogError($"Try to remove Subscriber [{subscriber.SubscriberName}] FAILED.");
            }
        }

        public void PublishMessage(ISubscriberCallback subscriber, IPublishableMessage message)
        {
            bool success = subscribers.TryGetValue(subscriber, out Queue<IPublishableMessage> queueOfMessages);

            if (success)
            {
                queueOfMessages.Enqueue(message);
                //TODO: check this log in particular
                Logger.LogDebug($"Published message [{message}] SUCCESSFYLLY enqueued on Subscriber [{subscriber.SubscriberName}]");
            }
            else
            {
                Logger.LogError($"Try to get queue of messages for Subscriber [{subscriber.SubscriberName}] FAILED.");
            }
        }

        public IPublishableMessage GetNextMessage(ISubscriberCallback subscriber)
        {
            IPublishableMessage message = null;

            bool success = subscribers.TryGetValue(subscriber, out Queue<IPublishableMessage> queueOfMessages) && queueOfMessages.Count > 0;

            if (success)
            {
                message = queueOfMessages.Dequeue();
                Logger.LogDebug($"Published message [{message}] SUCCESSFYLLY dequeued from Subscriber's queue of messages [Subscriber name: '{subscriber.SubscriberName}']");
            }
            else
            {
                Logger.LogError($"Try to get queue of messages for Subscriber [{subscriber.SubscriberName}] FAILED or queue is empty.");
            }

            return message;
        }
    }
}
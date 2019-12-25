using Outage.Common.PubSub;
using Outage.Common.ServiceContracts.PubSub;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PubSubEngine
{
    public class Subscribers
    {
        private ConcurrentDictionary<IPubSubNotification, Queue<IPublishableMessage>> subscribers;
        private static Subscribers instance;

        private Subscribers()
        {
            subscribers = new ConcurrentDictionary<IPubSubNotification, Queue<IPublishableMessage>>();
        }

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

        public bool TryAddSubscriber(IPubSubNotification subscriber)
        {
            return subscribers.TryAdd(subscriber, new Queue<IPublishableMessage>());
        }

        public void RemoveSubscriber(IPubSubNotification subscriber)
        {
            subscribers.TryRemove(subscriber, out Queue<IPublishableMessage> queue);
        }

        public void PublishMessage(IPubSubNotification subscriber, IPublishableMessage message)
        {
            if (subscribers.TryGetValue(subscriber, out Queue<IPublishableMessage> queueOfMessages))
            {
                queueOfMessages.Enqueue(message);
            }
        }

        public IPublishableMessage GetNextMessage(IPubSubNotification subscriber)
        {
            IPublishableMessage message = null;

            if (subscribers.TryGetValue(subscriber, out Queue<IPublishableMessage> queue) && queue.Count > 0)
            {
                message = queue.Dequeue();
            }

            return message;
        }
    }
}
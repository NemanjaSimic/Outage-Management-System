using Outage.Common;
using Outage.Common.ServiceContracts.PubSub;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PubSubEngine
{
    public class Publications
    {
        private ConcurrentDictionary<Topic, List<IPubSubNotification>> subscribedClients;
        private static Publications instance;

        private Publications()
        {
            subscribedClients = new ConcurrentDictionary<Topic, List<IPubSubNotification>>();
        }

        public static Publications Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Publications();
                }
                return instance;
            }
        }

        public bool TryAddSubscriber(Topic topic, IPubSubNotification subscriber)
        {
            bool success = subscribedClients.TryGetValue(topic, out List<IPubSubNotification> list);

            if (success)
            {
                list.Add(subscriber);
            }
            else
            {
                list = new List<IPubSubNotification>
                {
                    subscriber
                };
                success = subscribedClients.TryAdd(topic, list);
            }
            return success;
        }

        public void RemoveSubscriber(IPubSubNotification subscriber)
        {
            foreach (var item in subscribedClients)
            {
                if (item.Value.Contains(subscriber))
                {
                    item.Value.Remove(subscriber);
                }
            }
        }

        public List<IPubSubNotification> GetAllSubscribers(Topic topic)
        {
            subscribedClients.TryGetValue(topic, out List<IPubSubNotification> listOfSubscribers);
            return listOfSubscribers;
        }
    }
}
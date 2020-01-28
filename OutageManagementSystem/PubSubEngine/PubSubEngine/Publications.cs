using Outage.Common;
using Outage.Common.ServiceContracts.PubSub;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PubSubEngine
{
    public class Publications
    {
        private static ILogger Logger = LoggerWrapper.Instance;
        private ConcurrentDictionary<Topic, List<ISubscriberCallback>> subscribedClients;
        private static Publications instance;

        private Publications()
        {
            subscribedClients = new ConcurrentDictionary<Topic, List<ISubscriberCallback>>();
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

        public bool TryAddSubscriber(Topic topic, ISubscriberCallback subscriber, string subscriberName)
        {
            bool success = subscribedClients.TryGetValue(topic, out List<ISubscriberCallback> list);

            if (success)
            {
                list.Add(subscriber);
                Logger.LogDebug($"Subscriber [{subscriberName}] added to subscribed clients map. [Key Topic: {topic}]");
            }
            else if(!subscribedClients.ContainsKey(topic))
            {
                success = subscribedClients.TryAdd(topic, new List<ISubscriberCallback>(){subscriber});

                if(success)
                {
                    Logger.LogDebug($"Subscriber [{subscriberName}] added to subscribed clients map. [Key Topic: {topic}]");
                }
            }

            return success;
        }

        public void RemoveSubscriber(ISubscriberCallback subscriber)
        {
            string subscriberName = Subscribers.Instance.GetSubscriberName(subscriber);
            foreach (Topic topic in subscribedClients.Keys)
            {
                List<ISubscriberCallback> listOfSubscribers = subscribedClients[topic];

                if (listOfSubscribers.Contains(subscriber))
                {
                    if(listOfSubscribers.Remove(subscriber))
                    {
                        Logger.LogDebug($"Subscriber [] removed from subscribed clients map. [Key Topic: {topic}]");
                    }
                }
            }
        }

        public List<ISubscriberCallback> GetAllSubscribers(Topic topic)
        {
            bool success = subscribedClients.TryGetValue(topic, out List<ISubscriberCallback> listOfSubscribers);
            
            if(success)
            {
                Logger.LogDebug($"Try to get List of subscribers is SUCCESSFUL. Topic ['{topic}']");
            }
            else
            {
                Logger.LogDebug($"List of subscribers for topic: '{topic}' is empty.");
                listOfSubscribers = new List<ISubscriberCallback>();
            }
           
            return listOfSubscribers;
        }
    }
}
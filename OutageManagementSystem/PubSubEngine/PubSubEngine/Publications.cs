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

        public bool TryAddSubscriber(Topic topic, ISubscriberCallback subscriber)
        {
            bool success = subscribedClients.TryGetValue(topic, out List<ISubscriberCallback> list);
            string subscriberName = subscriber.GetSubscriberName();

            if (success)
            {
                list.Add(subscriber);
                Logger.LogDebug($"Subscriber [{subscriberName}] added to subscribed clients map. [Key Topic: {topic}]");
            }
            else if(!subscribedClients.ContainsKey(topic))
            {
                list = new List<ISubscriberCallback>
                {
                    subscriber
                };

                success = subscribedClients.TryAdd(topic, list);

                if(success)
                {
                    Logger.LogDebug($"Subscriber [{subscriberName}] added to subscribed clients map. [Key Topic: {topic}]");
                }
            }

            return success;
        }

        public void RemoveSubscriber(ISubscriberCallback subscriber)
        {
            //string subscriberName = subscriber.GetSubscriberName();

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
            else if(subscribedClients.ContainsKey(topic) && subscribedClients[topic].Count == 0)
            {
                Logger.LogDebug($"List of subscribers for topic: '{topic}' is empty.");
            }
            
            return listOfSubscribers;
        }
    }
}
using Outage.Common;
using Outage.Common.ServiceContracts.PubSub;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PubSubEngine
{
    public class Publications
    {
        private static ILogger logger = LoggerWrapper.Instance;
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

            if (success)
            {
                list.Add(subscriber);
                logger.LogDebug($"Subscriber [{subscriber.SubscriberName}] added to subscribed clients map. [Key Topic: {topic}]");
            }
            else
            {
                list = new List<ISubscriberCallback>
                {
                    subscriber
                };

                success = subscribedClients.TryAdd(topic, list);

                if(success)
                {
                    logger.LogDebug($"Subscriber [{subscriber.SubscriberName}] added to subscribed clients map. [Key Topic: {topic}]");
                }
                else
                {
                    logger.LogWarn($"Try to add Subscriber [{subscriber.SubscriberName}] to subscribed clients map FAILED. [Key Topic: {topic}]");
                }
            }

            return success;
        }

        public void RemoveSubscriber(ISubscriberCallback subscriber)
        {
            foreach (var item in subscribedClients)
            {
                if (item.Value.Contains(subscriber))
                {
                    item.Value.Remove(subscriber);
                    logger.LogInfo($"Subscriber [{subscriber.SubscriberName}] removed from subscribed clients map. [Key Topic: {item.Key}]");
                }
            }
        }

        public List<ISubscriberCallback> GetAllSubscribers(Topic topic)
        {
            bool success = subscribedClients.TryGetValue(topic, out List<ISubscriberCallback> listOfSubscribers);
            
            if(success)
            {
                logger.LogDebug($"Try to get List of subscribers is SUCCESSFUL. Topic ['{topic}']");
            }
            else
            {
                logger.LogError($"Try to get List of subscribers FAILED. Topic ['{topic}']");
            }
            
            return listOfSubscribers;
        }
    }
}
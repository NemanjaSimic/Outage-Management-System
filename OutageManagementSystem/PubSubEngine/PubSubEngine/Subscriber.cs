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
        private static ILogger Logger = LoggerWrapper.Instance;

        public void Subscribe(Topic topic)
        {
            ISubscriberCallback subscriber = OperationContext.Current.GetCallbackChannel<ISubscriberCallback>();
            string subscriberName = subscriber.GetSubscriberName();

            if (Subscribers.Instance.TryAddSubscriber(subscriber))
            {
                Logger.LogInfo($"Subscriber [{subscriberName}] added to list of all subscribers SUCCESSFULLY.");
                Thread thread = new Thread(() => TrackPublications(subscriber, subscriberName));
                thread.Start();
            }

            if (Publications.Instance.TryAddSubscriber(topic, subscriber))
            {
                string message = $"Subscriber [{subscriberName}], added to map Topic -> subscriber SUCCESSFULLY. Topic: '{topic}'.";
                Logger.LogInfo(message);
            }

        }

        private void TrackPublications(ISubscriberCallback subscriber, string subscriberName)
        {
            bool end = false;

            Logger.LogInfo($"Thread for tracking publications STARTED. Subscriber [{subscriberName}]");

            while (!end)
            {
                IPublishableMessage message = Subscribers.Instance.GetNextMessage(subscriber);

                if (message != null)
                {
                    try
                    {
                        subscriber.Notify(message);
                        Logger.LogDebug($"Subscriber [{subscriberName}] notified SUCCESSFULLY.");
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Exception on notifying Subscriber [{subscriberName}].", ex);

                        Subscribers.Instance.RemoveSubscriber(subscriber);
                        Publications.Instance.RemoveSubscriber(subscriber);
                        end = true;
                    }
                }

                Thread.Sleep(200);
            }

            Logger.LogInfo($"Thread for tracking publications STOPPED. Subscriber [{subscriberName}]");
        }
    }
}
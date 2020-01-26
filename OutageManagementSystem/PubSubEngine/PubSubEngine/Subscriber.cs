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
            string subscriberName = "";
            ISubscriberCallback subscriber = OperationContext.Current.GetCallbackChannel<ISubscriberCallback>();
            try
            {
                subscriberName = subscriber.GetSubscriberName();
            }
            catch (Exception ex)
            {
                Logger.LogDebug($"Couldn't get subscriber name. Execption message: {ex.Message}");
            }

            if (Subscribers.Instance.TryAddSubscriber(subscriber))
            {
                Logger.LogInfo($"Subscriber [{subscriberName}] added to list of all subscribers SUCCESSFULLY.");
                Thread thread = new Thread(() => TrackPublications(subscriber, subscriberName));
                thread.Start();
            }
            else
            {
                Logger.LogDebug($"Failed to add subscriber [{subscriberName}] to list of subsribers.");
            }

            if (Publications.Instance.TryAddSubscriber(topic, subscriber))
            {
                string message = $"Subscriber [{subscriberName}], added to map Topic -> subscriber SUCCESSFULLY. Topic: '{topic}'.";
                Logger.LogInfo(message);
            }
            else
            {
                Logger.LogDebug($"Failed to add subscriber [{subscriberName}] to map Topic -> subsriber. Topic: {topic}");
            }

        }

        private void TrackPublications(ISubscriberCallback subscriber, string subscriberName)
        {
            bool end = false;

            Logger.LogInfo($"Thread for tracking publications STARTED. Subscriber [{subscriberName}]");

            bool isMessageNull = false;

            while (!end)
            {
                IPublishableMessage message = Subscribers.Instance.GetNextMessage(subscriber, isMessageNull);

                isMessageNull = message == null;

                if (!isMessageNull)
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
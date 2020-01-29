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
            string subscriberName = "FAILED TO GET A NAME";
            ISubscriberCallback subscriber = OperationContext.Current.GetCallbackChannel<ISubscriberCallback>();
            try
            {
                subscriberName = subscriber.GetSubscriberName();

                if (Subscribers.Instance.TryAddSubscriber(subscriber, subscriberName))
                {
                    Logger.LogInfo($"Subscriber [{subscriberName}] added to list of all subscribers SUCCESSFULLY.");
                    Thread thread = new Thread(() => TrackPublications(subscriber, subscriberName));
                    thread.Start();
                }
                else
                {
                    Logger.LogDebug($"Failed to add subscriber [{subscriberName}] to list of subsribers.");
                }

                if (Publications.Instance.TryAddSubscriber(topic, subscriber, subscriberName))
                {
                    string message = $"Subscriber [{subscriberName}], added to map Topic -> subscriber SUCCESSFULLY. Topic: '{topic}'.";
                    Logger.LogInfo(message);
                }
                else
                {
                    Logger.LogWarn($"Failed to add subscriber [{subscriberName}] to map Topic -> subsriber. Topic: {topic}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogDebug($"Failed while subscribing. Execption message: {ex.Message}");
                Subscribers.Instance.RemoveSubscriber(subscriber);
            }

        }

        private void TrackPublications(ISubscriberCallback subscriber, string subscriberName)
        {
            Logger.LogInfo($"Thread for tracking publications STARTED. Subscriber [{subscriberName}]");
            bool end = false;
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
                    catch (Exception)
                    {
                        Logger.LogWarn($"Subscriber [{subscriberName}] is no longer in subscriber list.");
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
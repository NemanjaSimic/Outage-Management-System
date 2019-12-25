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
        public void Subscribe(Topic topic)
        {
            var subscriber = OperationContext.Current.GetCallbackChannel<IPubSubNotification>();

            Subscribers.Instance.TryAddSubscriber(subscriber);
            Publications.Instance.TryAddSubscriber(topic, subscriber);

            Thread thread = new Thread(() => TrackPublications(subscriber));
            thread.Start();
        }

        private void TrackPublications(IPubSubNotification subscriber)
        {
            bool end = false;
            while (!end)
            {
                IPublishableMessage message = Subscribers.Instance.GetNextMessage(subscriber);

                if (message != null)
                {
                    try
                    {
                        subscriber.Notify(message);
                    }
                    catch (Exception)
                    {
                        Subscribers.Instance.RemoveSubscriber(subscriber);
                        Publications.Instance.RemoveSubscriber(subscriber);
                        end = true;
                    }
                }

                Thread.Sleep(200);
            }
        }
    }
}
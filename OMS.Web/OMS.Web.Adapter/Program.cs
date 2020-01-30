namespace OMS.Web.Adapter
{
    using OMS.Web.Adapter.Topology;
    using OMS.Web.Common;
    using OMS.Web.Common.Mappers;
    using Outage.Common;
    using Outage.Common.ServiceProxies.PubSub;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    class Program
    {
        static void Main(string[] args)
        {
            SubscriberProxy _subscriberClient = new SubscriberProxy(
                new TopologyNotification("WEB_SUBSCRIBER",
                new GraphMapper()),
                EndpointNames.SubscriberEndpoint);

            var cancelTokenSource = new CancellationTokenSource();
            var cancelToken = cancelTokenSource.Token;
            try
            {
                Task.Factory.StartNew(() =>
                {
                    Retry.Do(
                        action: StartTopologySubscription,
                        retryInterval: TimeSpan.FromSeconds(1),
                        maxAttemptCount: 5);
                }, cancelToken);

            }
            catch (Exception)
            {
                Console.WriteLine("Failed to subscribe on Topology topic.");
            }

            Console.WriteLine("Press enter to close the app.");

            Console.ReadLine();
            cancelTokenSource.Cancel();
        }

        public static void StartTopologySubscription()
            => new SubscriberProxy(
                new TopologyNotification("WEB_SUBSCRIBER",
                new GraphMapper()),
                EndpointNames.SubscriberEndpoint).Subscribe(Topic.TOPOLOGY);

    }
}

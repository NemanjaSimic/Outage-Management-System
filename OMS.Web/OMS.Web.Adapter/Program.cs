using Outage.Common.ServiceProxies.PubSub;
using Outage.Common;

namespace OMS.Web.Adapter
{
    using OMS.Web.Adapter.Outage;
    using OMS.Web.Adapter.SCADA;
    using OMS.Web.Adapter.Topology;
    using OMS.Web.Common;
    using OMS.Web.Common.Mappers;
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


            SCADANotification scadaNotification = new SCADANotification("SCADA_ADAPTER_SUBSCRIBER");
            SubscriberProxy scadaSubscriberPRoxy = new SubscriberProxy(scadaNotification, EndpointNames.SubscriberEndpoint);

            try
            {
                scadaSubscriberPRoxy.Subscribe(Topic.MEASUREMENT);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception occured during scadaSubscriberPRoxy.Subscribe({Topic.MEASUREMENT}): {e.Message}");
                //neka retry logika ili nesto, ovako bezveze puca
                //throw; 
            }

            OutageNotification outageNotification = new OutageNotification("OUTAGE_ADAPTER_SUBSCRIBER", new OutageMapper(new ConsumerMapper()));
            SubscriberProxy outageSubscriberProxy = new SubscriberProxy(outageNotification, EndpointNames.SubscriberEndpoint);

            try
            {
                outageSubscriberProxy.Subscribe(Topic.ACTIVE_OUTAGE);
                outageSubscriberProxy.Subscribe(Topic.ARCHIVED_OUTAGE);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception occured during outageSubscriberProxy.Subscribe({Topic.ACTIVE_OUTAGE} or {Topic.ARCHIVED_OUTAGE}): {e.Message}");
                //neka retry logika ili nesto, ovako bezveze puca
                //throw; 
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

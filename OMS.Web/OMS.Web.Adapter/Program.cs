using OMS.Web.Adapter.SCADA;
using OMS.Web.Adapter.Topology;
using OMS.Web.Common;
using OMS.Web.Common.Mappers;
using Outage.Common;
using Outage.Common.ServiceProxies.PubSub;
using System;

namespace OMS.Web.Adapter
{
    class Program
    {
        static void Main(string[] args)
        {
            // ovo vise ne treba jer ne salje CE update na web nego na pubsub
            //WebServiceHost host = new WebServiceHost(AppSettings.Get<string>("webServiceUrl"));

            SubscriberProxy _subscriberClient = new SubscriberProxy(
                new TopologyNotification("WEB_SUBSCRIBER", new GraphMapper()),
                EndpointNames.SubscriberEndpoint
                );

            SubscriberProxy _subscriberSCADA = new SubscriberProxy(
                new SCADANotification("WEB_SUBSCRIBER"), EndpointNames.SCADAAnalogRecieverEndpoint);

            try
            {
                _subscriberClient.Subscribe(Topic.TOPOLOGY);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception occured during SubscriberClient.Subscribe(): {e.Message}");
                throw;
            }

            Console.WriteLine("Press enter to close the app.");
            Console.ReadLine();
        }
    }
}

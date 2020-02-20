using Outage.Common;
using Outage.Common.ServiceProxies.CalcualtionEngine;
using Outage.Common.ServiceProxies.Outage;
using Outage.Common.ServiceProxies.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopologyServiceClientMock;

namespace OMSTestClient
{
    class Program
    {
        static void Main(string[] args)
        {
			Subscriber sub = new Subscriber();
			try
			{
				using (var proxy = new OMSTopologyServiceProxy("TopologyServiceEndpoint"))
				{
					var ui = proxy.GetOMSModel();	
				}

				using (var proxy = new OutageSimulatorServiceProxy(EndpointNames.OutageSimulatorServiceEndpoint))
				{
					proxy.ResolvedOutage(0x0000000c00000029);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}

			//ProxyFactory proxyFactory = new ProxyFactory();
			//proxy = proxyFactory.CreatePRoxy<SubscriberProxy, ISubscriber>(new SCADASubscriber(), EndpointNames.SubscriberEndpoint);

			SubscriberProxy subProxy = new SubscriberProxy(sub, EndpointNames.SubscriberEndpoint);
			subProxy.Subscribe(Topic.OMS_MODEL);

			Console.ReadLine();
		}
    }
}

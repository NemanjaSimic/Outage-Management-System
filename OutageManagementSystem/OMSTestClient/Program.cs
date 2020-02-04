using Outage.Common;
using Outage.Common.ServiceProxies.CalcualtionEngine;
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
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			SubscriberProxy subProxy = new SubscriberProxy(sub, EndpointNames.SubscriberEndpoint);
			subProxy.Subscribe(Topic.OMS_MODEL);

			Console.ReadLine();
		}
    }
}

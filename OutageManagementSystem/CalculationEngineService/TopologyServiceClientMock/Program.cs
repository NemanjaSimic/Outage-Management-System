using Outage.Common.ServiceProxies.PubSub;
using Outage.Common;
using System;
using Outage.Common.ServiceProxies.CalcualtionEngine;
using Outage.Common.ServiceProxies;
using Outage.Common.ServiceContracts.CalculationEngine;

namespace TopologyServiceClientMock
{
	class Program
    {
        static void Main(string[] args)
        {
			ProxyFactory proxyFactory = new ProxyFactory();
			long elementGid;
			do
			{
				Console.WriteLine("Enter element gid:");
				if (long.TryParse(Console.ReadLine(), out elementGid))
				{
					using (MeasurementMapProxy proxy = proxyFactory.CreateProxy<MeasurementMapProxy, IMeasurementMapContract>(EndpointNames.MeasurementMapEndpoint))
					{
						var measurements = proxy.GetMeasurementsForElement(elementGid);
						if (measurements.Count > 0)
						{
							foreach (long measurementId in measurements)
							{
								Console.WriteLine($"Masurement gid {measurementId}");
							}
						}
						else
						{
							Console.WriteLine("There is no measurements for element.");
						}
					}
				}
				
			} while (true);

			//Subscriber sub = new Subscriber();
			//try
			//{
			//	using (var proxy = new TopologyServiceProxy("TopologyServiceEndpoint"))
			//	{
			//		var ui = proxy.GetTopology();
			//		sub.PrintUI(ui);
			//	}
			//}
			//catch (Exception ex)
			//{
			//	Console.WriteLine(ex.Message);
			//}

			//ProxyFactory proxyFactory = new ProxyFactory();
			//proxy = proxyFactory.CreatePRoxy<SubscriberProxy, ISubscriber>(new SCADASubscriber(), EndpointNames.SubscriberEndpoint);

			//SubscriberProxy subProxy = new SubscriberProxy(sub, EndpointNames.SubscriberEndpoint);
			//subProxy.Subscribe(Topic.TOPOLOGY);
			//subProxy.Subscribe(Topic.MEASUREMENT);
			//Console.ReadLine();

        }

	}
}

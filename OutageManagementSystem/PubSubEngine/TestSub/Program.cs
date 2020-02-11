using Outage.Common;
using Outage.Common.ServiceProxies.PubSub;
using System;

namespace TestSub
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				Console.WriteLine("Created..");
				Notification notification = new Notification("TEST_SUBSCRIBER");

				//ProxyFactory proxyFactory = new ProxyFactory();
				//proxy = proxyFactory.CreatePRoxy<SubscriberProxy, ISubscriber>(new SCADASubscriber(), EndpointNames.SubscriberEndpoint);

				SubscriberProxy proxy = new SubscriberProxy(notification, EndpointNames.SubscriberEndpoint);
				proxy.Subscribe(Topic.MEASUREMENT);
				proxy.Subscribe(Topic.SWITCH_STATUS);
				proxy.Subscribe(Topic.ACTIVE_OUTAGE);

				Console.WriteLine("Subscribed..");
			}
			catch (Exception e)
			{

				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}

			Console.ReadLine();
		}
	}
}

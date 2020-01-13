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
				Notification notification1 = new Notification("MEASUREMENT_SUBSCRIBER");
				SubscriberProxy proxy1 = new SubscriberProxy(notification1, EndpointNames.SubscriberEndpoint);
				proxy1.Subscribe(Topic.MEASUREMENT);

                Notification notification2 = new Notification("SWITCH_STATUS_SUBSCRIBER");
                SubscriberProxy proxy2 = new SubscriberProxy(notification2, EndpointNames.SubscriberEndpoint);
                proxy2.Subscribe(Topic.SWITCH_STATUS);

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

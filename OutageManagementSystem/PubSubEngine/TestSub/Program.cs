using PubSubCommon;
using PubSubCommon.Proxy;
using System;
using System.ServiceModel;
using static PubSubCommon.Enums;

namespace TestSub
{
	class Program
	{
		static void Main(string[] args)
		{
			//DuplexChannelFactory<ISubscriber> factory = new DuplexChannelFactory<ISubscriber>(new InstanceContext(new Notify()), "PubSubService");
			//ISubscriber proxy = factory.CreateChannel();
			Console.WriteLine("Created..");
			//proxy.Subscribe(Topic.Status);
			var proxy = new SubscriberProxy(new Notify(), "PubSubService");
			
			proxy.Subscribe(Topic.Measurement);
			
			Console.WriteLine("Subscribed..");

			Console.ReadLine();
		}
	}
}

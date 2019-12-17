using PubSubCommon;
using PubSubCommon.Proxy;
using System;
using System.ServiceModel;
using static PubSubCommon.Enums;

namespace Sub
{
	class Program
	{
		static void Main(string[] args)
		{
			//DuplexChannelFactory<ISubscriber> factory = new DuplexChannelFactory<ISubscriber>(new InstanceContext(new Notify()),"PubSubService");
			//ISubscriber proxy = factory.CreateChannel();
			Console.WriteLine("Created..");
			var proxy = new SubscriberProxy(new Notify(), "PubSubService");
			
			proxy.Subscribe(Topic.Measurement);
			
			//proxy.Subscribe(Topic.Measurement);
			Console.WriteLine("Subscribed..");

			Console.ReadLine();
		}
	}
}

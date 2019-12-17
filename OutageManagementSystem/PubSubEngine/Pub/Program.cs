using System;
using System.ServiceModel;
using System.Threading;
using PubSubCommon;
using PubSubCommon.Proxy;
using static PubSubCommon.Enums;

namespace Pub
{
	class Program
	{
		static void Main(string[] args)
		{
			//ChannelFactory<IPublisher> factory = new ChannelFactory<IPublisher>("PubSubService");
			//IPublisher proxy = factory.CreateChannel();
			Console.WriteLine("Connected..");
			var proxy = new PublisherProxy("PubSubService");

			Thread thread = new Thread(() => Send(proxy));
			thread.Start();
			for (int i = 0; i < 100; i++)
			{
				Thread.Sleep(1000);

				proxy.Publish(new Publication(PubSubCommon.Enums.Topic.Measurement, ("Hello " + i)));
				Console.WriteLine($"Message \" Hello {i} \" sent.");
			}



			Console.ReadLine();
		}

		static void Send(IPublisher proxy)
		{
			for (int i = 0; i < 100; i++)
			{
				Thread.Sleep(1000);

				proxy.Publish(new Publication(Topic.Status, ("Hello " + i)));
				Console.WriteLine($"Message \" Hello {i} \" sent.");
			}
		}
	}
}

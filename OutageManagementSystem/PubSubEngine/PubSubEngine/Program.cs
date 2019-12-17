using System;
using System.ServiceModel;

namespace PubSubEngine
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Started..");
			ServiceHost host = new ServiceHost(typeof(Publisher));
			host.Open();

			ServiceHost host2 = new ServiceHost(typeof(Subscriber));
			host2.Open();

			Console.ReadLine();
		}
	}
}

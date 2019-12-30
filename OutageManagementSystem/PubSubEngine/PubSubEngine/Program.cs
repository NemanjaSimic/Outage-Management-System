using System;
using System.ServiceModel;

namespace PubSubEngine
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				Console.WriteLine("Started..");
				ServiceHost host = new ServiceHost(typeof(Publisher));
				host.Open();

				ServiceHost host2 = new ServiceHost(typeof(Subscriber));
				host2.Open();
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

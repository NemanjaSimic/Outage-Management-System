using System;
using System.ServiceModel;
using PubSubCommon;

namespace TestSub
{
	[CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
	public class Notify : INotify
	{
		void INotify.Notify(string msg)
		{
			Console.WriteLine("Message from PubSub: " + msg);
		}
	}
}

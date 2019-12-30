using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using Outage.Common.PubSub;
using Outage.Common.ServiceContracts.PubSub;

namespace TestSub
{
	[CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
	[DataContract]
	public class Notification : ISubscriberCallback
	{
		[DataMember]
		public string SubscriberName { get; private set; }

		public Notification(string subscriberName = "")
		{
			SubscriberName = subscriberName;
		}

		public void Notify(IPublishableMessage msg)
		{
			Console.WriteLine("Message from PubSub: " + msg);
		}
	}
}

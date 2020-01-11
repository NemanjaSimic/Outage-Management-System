using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using Outage.Common.PubSub;
using Outage.Common.ServiceContracts.PubSub;
using Outage.Common.PubSub.SCADADataContract;

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
			
			if(msg is MultipleAnalogValueSCADAMessage multipleAnalogValue)
			{
                foreach(long gid in multipleAnalogValue.Data.Keys)
                {
                    int currentValue = multipleAnalogValue.Data[gid];
                    Console.WriteLine($"Analog => Gid: {gid}, Value: {currentValue}");
                }
			}
            else if(msg is MultipleDiscreteValueSCADAMessage multipleDiscreteValue)
            {
                foreach (long gid in multipleDiscreteValue.Data.Keys)
                {
                    bool currentValue = multipleDiscreteValue.Data[gid];
                    Console.WriteLine($"Discrete => Gid: {gid}, Value: {currentValue}");
                }
            }
		}
	}
}

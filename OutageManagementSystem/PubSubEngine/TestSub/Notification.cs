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
        private string subscriberName;

		public Notification(string subscriberName = "")
		{
			this.subscriberName = subscriberName;
		}

        public string GetSubscriberName()
        {
            return subscriberName;
        }

        public void Notify(IPublishableMessage msg)
		{
			Console.WriteLine("Message from PubSub: " + msg);
			
			if(msg is MultipleAnalogValueSCADAMessage multipleAnalogValue)
			{
                foreach(long gid in multipleAnalogValue.Data.Keys)
                {
                    int currentValue = multipleAnalogValue.Data[gid];
                    Console.WriteLine($"Analog => Gid: 0x{gid:X16}, Value: {currentValue}");
                }
			}
            else if(msg is MultipleDiscreteValueSCADAMessage multipleDiscreteValue)
            {
                foreach (long gid in multipleDiscreteValue.Data.Keys)
                {
                    bool currentValue = multipleDiscreteValue.Data[gid];
                    Console.WriteLine($"Discrete => Gid: 0x{gid:X16}, Value: {currentValue}");
                }
            }
		}
	}
}

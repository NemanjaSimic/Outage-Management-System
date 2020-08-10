using Common.PubSub;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.PubSubContracts;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OMS.OutageLifecycleServiceImplementation.ScadaSub
{
	public class ScadaSubscriber : INotifySubscriberContract
	{
		private ICloudLogger logger;

		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}

		public long HeadBreakerID { get; set; }
		public string SubscriberName { get; set; }
		public AutoResetEvent AutoResetEvent { get; set; }

		public ScadaSubscriber(string subscriberName)
		{
			HeadBreakerID = -1;
			AutoResetEvent = null;

			SubscriberName = subscriberName;
		}

		public async Task<string> GetSubscriberName()
		{
			return SubscriberName;
		}

		public async Task Notify(IPublishableMessage message, string publisherName)
		{
			if (message is MultipleDiscreteValueSCADAMessage multipleDiscreteValueSCADAMessage)
			{
				if (HeadBreakerID == -1 || AutoResetEvent == null)
				{
					Logger.LogWarning("Received MultipleDiscreteValueSCADAMessage, but HeadBreakerID or AutoResetEvent is not set");
					return;
				}

				Logger.LogDebug("ScadaNotify from outage.");
				if (multipleDiscreteValueSCADAMessage.Data.ContainsKey(HeadBreakerID))
				{
					if (multipleDiscreteValueSCADAMessage.Data[HeadBreakerID].Value == (ushort)DiscreteCommandingType.OPEN)
					{
						Logger.LogDebug("ScadaNotify from outage, open command.");
						AutoResetEvent.Set();
					}

				}

				AutoResetEvent = null;
				HeadBreakerID = -1;
			}
		}
	}
}

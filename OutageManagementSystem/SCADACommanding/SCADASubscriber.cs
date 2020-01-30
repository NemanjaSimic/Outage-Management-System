using CECommon.Providers;
using Outage.Common;
using Outage.Common.PubSub;
using Outage.Common.PubSub.SCADADataContract;
using Outage.Common.ServiceContracts.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SCADACommanding
{
	[CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
	public class SCADASubscriber : ISubscriberCallback
	{
		private ILogger Logger = LoggerWrapper.Instance;
		public string GetSubscriberName() => "Calculation Engine";

		public void Notify(IPublishableMessage message)
		{
			Logger.LogError($"Message recived from PubSub with type {message.GetType().ToString()}.");
			Provider.Instance.SCADAResultHandler.HandleResult(message);
		}
	}
}

using CECommon.Providers;
using Outage.Common;
using Outage.Common.PubSub;
using Outage.Common.ServiceContracts.PubSub;
using System.ServiceModel;

namespace SCADACommanding
{
	[CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
	public class SCADASubscriber : ISubscriberCallback
	{
		private ILogger Logger = LoggerWrapper.Instance;
		public string GetSubscriberName() => "Calculation Engine";

		public void Notify(IPublishableMessage message)
		{
			Logger.LogDebug($"Message recived from PubSub with type {message.GetType().ToString()}.");	
			Provider.Instance.SCADAResultHandler.HandleResult(message);
		}
	}
}

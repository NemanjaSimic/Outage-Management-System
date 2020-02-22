using Outage.Common.OutageService.Interface;
using Outage.Common.PubSub;
using Outage.Common.ServiceContracts.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace OMSTestClient
{
	[CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
	class Subscriber : ISubscriberCallback
	{
		public string GetSubscriberName()
		{
			return "Test client mock";
		}

		public void Notify(IPublishableMessage message)
		{
			IOutageTopologyModel outageTopologyModel = message as IOutageTopologyModel;
		}

	}
}

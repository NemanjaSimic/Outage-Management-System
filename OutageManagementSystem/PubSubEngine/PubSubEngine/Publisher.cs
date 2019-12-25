using System.Collections.Generic;
using System.ServiceModel;
using Outage.Common.PubSub;
using Outage.Common.ServiceContracts.PubSub;
using PubSubCommon;

namespace PubSubEngine
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
	class Publisher : IPublisher
	{
		public void Publish(IPublication publication)
		{
			List<INotify> listOfSubscribers =  Publications.Instance.GetAllSubscribers(publication.Topic);

			if (listOfSubscribers != null)
			{
				foreach (var item in listOfSubscribers)
				{
					Subscribers.Instance.PublishMessage(item, publication.Message);
				}
			}

		}

	}
}

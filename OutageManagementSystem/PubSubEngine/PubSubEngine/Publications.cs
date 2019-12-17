using System.Collections.Concurrent;
using System.Collections.Generic;
using PubSubCommon;
using static PubSubCommon.Enums;

namespace PubSubEngine
{
	public class Publications
	{
		private ConcurrentDictionary<Topic, List<INotify>> subscribedClients;
		private static Publications instance;

		private Publications()
		{
			subscribedClients = new ConcurrentDictionary<Topic, List<INotify>>();
		}

		public static Publications Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new Publications();
				}
				return instance;
			}
		}

		public bool TryAddSubscriber(Topic topic, INotify subscriber)
		{
			bool success = subscribedClients.TryGetValue(topic, out List<INotify> list);

			if (success)
			{
				list.Add(subscriber);
			}
			else
			{
				list = new List<INotify>
				{
					subscriber
				};
				success = subscribedClients.TryAdd(topic, list);
			}
			return success;
		}

		public void RemoveSubscriber(INotify subscriber)
		{
			foreach (var item in subscribedClients)
			{
				if (item.Value.Contains(subscriber))
				{
					item.Value.Remove(subscriber);
				}
			}
		}

		public List<INotify> GetAllSubscribers(Topic topic)
		{
			subscribedClients.TryGetValue(topic, out List<INotify> listOfSubscribers);
			return listOfSubscribers;
		}

	}
}

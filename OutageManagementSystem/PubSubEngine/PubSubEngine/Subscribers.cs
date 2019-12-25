using Outage.Common.PubSub;
using Outage.Common.ServiceContracts.PubSub;
using PubSubCommon;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PubSubEngine
{
	public class Subscribers
	{
		private ConcurrentDictionary<INotify, Queue<IPublishableMessage>> subscribers;
		private static Subscribers instance;

		private Subscribers()
		{
			subscribers = new ConcurrentDictionary<INotify, Queue<IPublishableMessage>>();
		}

		public static Subscribers Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new Subscribers();
				}
				return instance;
			}
		}

		public bool TryAddSubscriber(INotify subscriber)
		{
			return subscribers.TryAdd(subscriber, new Queue<IPublishableMessage>());
		}

		public void RemoveSubscriber(INotify subscriber)
		{
			subscribers.TryRemove(subscriber, out Queue<IPublishableMessage> queue);
		}

		public void PublishMessage(INotify subscriber, IPublishableMessage message)
		{
			if (subscribers.TryGetValue(subscriber, out Queue<IPublishableMessage> queueOfMessages))
			{
				queueOfMessages.Enqueue(message);
			}
		}

		public IPublishableMessage GetNextMessage(INotify subscriber)
		{
            IPublishableMessage message = null;

            if (subscribers.TryGetValue(subscriber, out Queue<IPublishableMessage> queue) && queue.Count > 0)
			{
				message = queue.Dequeue();
			}

			return message;
		}
	}
}

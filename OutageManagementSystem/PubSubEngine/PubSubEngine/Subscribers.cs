using PubSubCommon;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PubSubEngine
{
	public class Subscribers
	{
		private ConcurrentDictionary<INotify, Queue<string>> subscribers;
		private static Subscribers instance;

		private Subscribers()
		{
			subscribers = new ConcurrentDictionary<INotify, Queue<string>>();
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
			return subscribers.TryAdd(subscriber, new Queue<string>());
		}

		public void RemoveSubscriber(INotify subscriber)
		{
			subscribers.TryRemove(subscriber, out Queue<string> queue);
		}

		public void PublishMessage(INotify subscriber,string message)
		{
			if (subscribers.TryGetValue(subscriber, out Queue<string> queueOfMessages))
			{
				queueOfMessages.Enqueue(message);
			}
		}

		public string GetNextMessage(INotify subscriber)
		{
			string message = String.Empty;
			if (subscribers.TryGetValue(subscriber, out Queue<string> queue) && queue.Count > 0)
			{
				message = queue.Dequeue();
			}
			return message;
		}
	}
}

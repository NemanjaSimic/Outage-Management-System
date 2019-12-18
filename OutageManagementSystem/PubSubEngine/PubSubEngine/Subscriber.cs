﻿using PubSubCommon;
using System;
using System.ServiceModel;
using System.Threading;
using static PubSubCommon.Enums;

namespace PubSubEngine
{
	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Reentrant)]
	class Subscriber : ISubscriber
	{
		public void Subscribe(Topic topic)
		{
			var subscriber = OperationContext.Current.GetCallbackChannel<INotify>();

			Subscribers.Instance.TryAddSubscriber(subscriber);
			Publications.Instance.TryAddSubscriber(topic, subscriber);

			Thread thread = new Thread(() => Publish(subscriber));
			thread.Start();
		}

		private void Publish(INotify subscriber)
		{
			bool end = false;
			while (!end)
			{
				string message = Subscribers.Instance.GetNextMessage(subscriber);
				if (!String.IsNullOrEmpty(message))
				{
					try
					{
						subscriber.Notify(message);
						Thread.Sleep(200);
					}
					catch (Exception)
					{
						Subscribers.Instance.RemoveSubscriber(subscriber);
						Publications.Instance.RemoveSubscriber(subscriber);
						end = true;
					}
				}
			}
		}
	}
}
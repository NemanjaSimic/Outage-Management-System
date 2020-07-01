using Common.PubSub;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.PubSub;
using OMS.Common.PubSubContracts;
using OMS.Common.WcfClient.PubSub;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PubSubImplementation
{
    public class PublisherProvider : IPublisherContract
    {
        private readonly string baseLogString;
        private readonly IReliableStateManager stateManager;

        #region Private Properties
        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        private bool isSubscriberCacheInitialized;
        private bool ReliableDictionariesInitialized
        {
            get { return isSubscriberCacheInitialized; }
        }

        private ReliableDictionaryAccess<short, Dictionary<Uri, RegisteredSubscriber>> registeredSubscribersCache;
        private ReliableDictionaryAccess<short, Dictionary<Uri, RegisteredSubscriber>> RegisteredSubscribersCache
        {
            get
            {
                return registeredSubscribersCache ?? (registeredSubscribersCache = ReliableDictionaryAccess<short, Dictionary<Uri, RegisteredSubscriber>>.Create(stateManager, ReliableDictionaryNames.RegisteredSubscribersCache).Result);
            }
        }
        #endregion Private Properties

        public PublisherProvider(IReliableStateManager stateManager)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>";

            this.isSubscriberCacheInitialized = false;

            this.stateManager = stateManager;
            this.stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;
        }

        private async void OnStateManagerChangedHandler(object sender, NotifyStateManagerChangedEventArgs e)
        {
            if (e.Action == NotifyStateManagerChangedAction.Add)
            {
                var operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
                string reliableStateName = operation.ReliableState.Name.AbsolutePath;

                if (reliableStateName == ReliableDictionaryNames.RegisteredSubscribersCache)
                {
                    //_ = SubscriberCache;
                    registeredSubscribersCache = await ReliableDictionaryAccess<short, Dictionary<Uri, RegisteredSubscriber>>.Create(stateManager, ReliableDictionaryNames.RegisteredSubscribersCache);
                    isSubscriberCacheInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.RegisteredSubscribersCache}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
            }
        }

        #region IPublisherContract
        public async Task<bool> Publish(IPublication publication, Uri publisherUri)
        {
            while (!ReliableDictionariesInitialized)
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            ///Could be sole Task running in RunAsync, while reading from a queue...
            List<Task> tasks = new List<Task>();
            short key = (short)publication.Topic;

            if (RegisteredSubscribersCache.ContainsKey(key))
            {
                var registeredSubscribers = RegisteredSubscribersCache[key];
                
                foreach(var subscriber in registeredSubscribers.Values)
                {
                    var notifySubscriberClient = NotifySubscriberClient.CreateClient(subscriber.SubcriberUri, subscriber.ServiceType);
                    var task = notifySubscriberClient.Notify(publication.Message); 
                    tasks.Add(task);
                }
            }
            ///////////

            Task.WaitAll(tasks.ToArray());

            return true;
        }
        #endregion IPublisherContract
    }
}

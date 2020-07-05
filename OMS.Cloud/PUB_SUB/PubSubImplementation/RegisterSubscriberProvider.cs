using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.PubSub;
using OMS.Common.PubSubContracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PubSubImplementation
{
    public class RegisterSubscriberProvider : IRegisterSubscriberContract
    {
        private readonly string baseLogString;
        private readonly IReliableStateManager stateManager;

        #region Private Properties
        private bool isSubscriberCacheInitialized;
        private bool ReliableDictionariesInitialized
        {
            get { return isSubscriberCacheInitialized; }
        }

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        private ReliableDictionaryAccess<short, Dictionary<Uri, RegisteredSubscriber>> registeredSubscribersCache;
        private ReliableDictionaryAccess<short, Dictionary<Uri, RegisteredSubscriber>> RegisteredSubscribersCache
        {
            get { return registeredSubscribersCache; }
        }
        #endregion Properties

        public RegisterSubscriberProvider(IReliableStateManager stateManager)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

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
                }
            }
        }

        #region IRegisterSubscriberContract
        public async Task<bool> SubscribeToTopic(Topic topic, Uri subscriberUri, ServiceType serviceType)
        {
            while (!ReliableDictionariesInitialized)
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            bool success;

            try
            {
                short key = (short)topic;
                if (!(await RegisteredSubscribersCache.ContainsKeyAsync(key)))
                {
                    await RegisteredSubscribersCache.SetAsync(key, new Dictionary<Uri, RegisteredSubscriber>());
                }

                var subscriber = new RegisteredSubscriber(subscriberUri, serviceType);

                var result = await RegisteredSubscribersCache.TryGetValueAsync(key);

                if(!result.HasValue)
                {
                    return false;
                }
                
                var subscribers = result.Value;

                if(subscribers.ContainsKey(subscriberUri))
                {
                    return false;
                }
                
                subscribers.Add(subscriber.SubcriberUri, subscriber);
                await RegisteredSubscribersCache.SetAsync(key, subscribers);

                string debugMessage = $"{baseLogString} SubscribeToTopic => {subscriberUri} SUCCESSFULLY subscribed to topic '{topic}'.";
                Logger.LogDebug(debugMessage);
                success = true;
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} SubscribeToTopic => Exception: {e.Message}";
                Logger.LogError(errorMessage, e);
                success = false;
            }

            return success;
        }

        public async Task<bool> SubscribeToTopics(IEnumerable<Topic> topics, Uri subscriberUri, ServiceType serviceType)
        {
            while (!ReliableDictionariesInitialized)
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            bool success;

            try
            {
                foreach (Topic topic in topics)
                {
                    short key = (short)topic;
                    if (!(await RegisteredSubscribersCache.ContainsKeyAsync(key)))
                    {
                        await RegisteredSubscribersCache.SetAsync(key, new Dictionary<Uri, RegisteredSubscriber>());
                    }

                    var subscriber = new RegisteredSubscriber(subscriberUri, serviceType);

                    var result = await RegisteredSubscribersCache.TryGetValueAsync(key);

                    if (!result.HasValue)
                    {
                        return false;
                    }
                    
                    var subscribers = result.Value;

                    if (!subscribers.ContainsKey(subscriberUri))
                    {
                        subscribers.Add(subscriber.SubcriberUri, subscriber);
                        await RegisteredSubscribersCache.SetAsync(key, subscribers);

                        string debugMessage = $"{baseLogString} SubscribeToTopics => {subscriberUri} SUCCESSFULLY subscribed to topic '{topic}'.";
                        Logger.LogDebug(debugMessage);
                    }
                }

                success = true;
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} SubscribeToTopics => Exception: {e.Message}";
                Logger.LogError(errorMessage, e);
                success = false;
            }

            return success;
        }

        public async Task<HashSet<Topic>> GetAllSubscribedTopics(Uri subcriberUri)
        {
            while (!ReliableDictionariesInitialized)
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            try
            {
                var result = new HashSet<Topic>();

                var enumerableSubscribersCache = await RegisteredSubscribersCache.GetEnumerableDictionaryAsync();

                foreach (var kvp in enumerableSubscribersCache)
                {
                    var topic = kvp.Key;
                    var subscribers = kvp.Value;

                    if (subscribers.ContainsKey(subcriberUri))
                    {
                        result.Add((Topic)topic);
                    }
                }
                
                return result;
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} GetAllSubscribedTopics => Exception: {e.Message}";
                Logger.LogError(errorMessage, e);
                throw e;
            }
        }    

        public async Task<bool> UnsubscribeFromTopic(Topic topic, Uri subscriberUri)
        {
            while (!ReliableDictionariesInitialized)
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            bool success;

            try
            {
                short key = (short)topic;
                var result = await RegisteredSubscribersCache.TryGetValueAsync(key);

                if (!result.HasValue)
                {
                    return false;
                }
                
                var subscribers = result.Value;

                if (!subscribers.ContainsKey(subscriberUri))
                {
                    return false;
                }

                subscribers.Remove(subscriberUri);
                await RegisteredSubscribersCache.SetAsync(key, subscribers);

                string debugMessage = $"{baseLogString} UnsubscribeFromTopic => {subscriberUri} SUCCESSFULLY unsubscribed from topic '{topic}'.";
                Logger.LogDebug(debugMessage);
                success = true;
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} UnsubscribeFromTopic => Exception: {e.Message}";
                Logger.LogError(errorMessage, e);
                success = false;
            }

            return success;
        }

        public async Task<bool> UnsubscribeFromTopics(IEnumerable<Topic> topics, Uri subscriberUri)
        {
            while (!ReliableDictionariesInitialized)
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            bool success;

            try
            {
                foreach (Topic topic in topics)
                {
                    short key = (short)topic;
                    var result = await RegisteredSubscribersCache.TryGetValueAsync(key);

                    if (!result.HasValue)
                    {
                        continue;
                    }

                    var subscribers = result.Value;

                    if (!subscribers.ContainsKey(subscriberUri))
                    {
                        continue;
                    }

                    subscribers.Remove(subscriberUri);
                    await RegisteredSubscribersCache.SetAsync(key, subscribers);

                    string debugMessage = $"{baseLogString} UnsubscribeFromTopics => {subscriberUri} SUCCESSFULLY unsubscribed from topic '{topic}'.";
                    Logger.LogDebug(debugMessage);
                }

                success = true;
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} UnsubscribeFromTopics => Exception: {e.Message}";
                Logger.LogError(errorMessage, e);
                success = false;
            }

            return success;
        }

        public async Task<bool> UnsubscribeFromAllTopics(Uri subscriberUri)
        {
            while (!ReliableDictionariesInitialized)
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            bool result;

            try
            {
                var enumerableSubscribersCache = await RegisteredSubscribersCache.GetEnumerableDictionaryAsync();
                foreach (var topicKey in enumerableSubscribersCache.Keys)
                {
                    var subscribers = enumerableSubscribersCache[topicKey];

                    if (!subscribers.ContainsKey(subscriberUri))
                    {
                        continue;
                    }

                    subscribers.Remove(subscriberUri);
                    await RegisteredSubscribersCache.SetAsync(topicKey, subscribers);

                    string debugMessage = $"{baseLogString} UnsubscribeFromAllTopics => {subscriberUri} SUCCESSFULLY unsubscribed from topic '{(Topic)topicKey}'.";
                    Logger.LogDebug(debugMessage);
                }

                result = true;
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} UnsubscribeFromAllTopics => Exception: {e.Message}";
                Logger.LogError(errorMessage, e);
                result = false;
            }

            return result;
        }
        #endregion IRegisterSubscriberContract
    }
}

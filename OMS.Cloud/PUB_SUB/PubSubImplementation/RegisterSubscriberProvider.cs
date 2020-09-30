using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.PubSub;
using OMS.Common.PubSubContracts;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading.Tasks;

namespace PubSubImplementation
{
    public class RegisterSubscriberProvider : IRegisterSubscriberContract
    {
        private readonly string baseLogString;
        private readonly IReliableStateManager stateManager;

        #region Private Properties
        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }
        #endregion Properties

        #region Reliable Dictionary
        private bool isSubscriberCacheInitialized;
        private bool ReliableDictionariesInitialized
        {
            get { return isSubscriberCacheInitialized; }
        }

        private ReliableDictionaryAccess<short, HashSet<string>> registeredSubscribersCache;
        /// <summary>
        /// key - topic enumeration
        /// value - hashset of registered subscriber names (from MicroserviceNames)
        /// </summary>
        private ReliableDictionaryAccess<short, HashSet<string>> RegisteredSubscribersCache
        {
            get { return registeredSubscribersCache; }
        }

        private async void OnStateManagerChangedHandler(object sender, NotifyStateManagerChangedEventArgs eventArgs)
        {
            try
            {
                await InitializeReliableCollections(eventArgs);
            }
            catch (FabricNotPrimaryException)
            {
                Logger.LogDebug($"{baseLogString} OnStateManagerChangedHandler => NotPrimaryException. To be ignored.");
            }
        }

        private async Task InitializeReliableCollections(NotifyStateManagerChangedEventArgs e)
        {
            if (e.Action == NotifyStateManagerChangedAction.Add)
            {
                var operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
                string reliableStateName = operation.ReliableState.Name.AbsolutePath;

                if (reliableStateName == ReliableDictionaryNames.RegisteredSubscribersCache)
                {
                    //_ = SubscriberCache;
                    registeredSubscribersCache = await ReliableDictionaryAccess<short, HashSet<string>>.Create(stateManager, ReliableDictionaryNames.RegisteredSubscribersCache);
                    isSubscriberCacheInitialized = true;
                }
            }
        }
        #endregion Reliable Dictionary

        public RegisterSubscriberProvider(IReliableStateManager stateManager)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            this.isSubscriberCacheInitialized = false;

            this.stateManager = stateManager;
            this.stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;
        }

        #region IRegisterSubscriberContract
        public async Task<bool> SubscribeToTopic(Topic topic, string subscriberServiceName)
        {
            bool success;
            
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            try
            {

                HashSet<string> subscribers;
                short key = (short)topic;
                var result = await RegisteredSubscribersCache.TryGetValueAsync(key);

                if (!result.HasValue)
                {
                    await RegisteredSubscribersCache.SetAsync(key, new HashSet<string>());
                    subscribers = new HashSet<string>();
                }
                else
                {
                    subscribers = result.Value;
                }

                if(subscribers.Contains(subscriberServiceName))
                {
                    return false;
                }
                
                subscribers.Add(subscriberServiceName);
                await RegisteredSubscribersCache.SetAsync(key, subscribers);

                string debugMessage = $"{baseLogString} SubscribeToTopic => {subscriberServiceName} SUCCESSFULLY subscribed to topic '{topic}'.";
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

        public async Task<bool> SubscribeToTopics(IEnumerable<Topic> topics, string subscriberServiceName)
        {
            bool success;
            
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            try
            {
                foreach (Topic topic in topics)
                {
                    short key = (short)topic;
                    if (!(await RegisteredSubscribersCache.ContainsKeyAsync(key)))
                    {
                        await RegisteredSubscribersCache.SetAsync(key, new HashSet<string>());
                    }

                    var result = await RegisteredSubscribersCache.TryGetValueAsync(key);

                    if (!result.HasValue)
                    {
                        return false;
                    }
                    
                    var subscribers = result.Value;

                    if (!subscribers.Contains(subscriberServiceName))
                    {
                        subscribers.Add(subscriberServiceName);
                        await RegisteredSubscribersCache.SetAsync(key, subscribers);

                        string debugMessage = $"{baseLogString} SubscribeToTopics => {subscriberServiceName} SUCCESSFULLY subscribed to topic '{topic}'.";
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

        public async Task<HashSet<Topic>> GetAllSubscribedTopics(string subscriberServiceName)
        {
            var result = new HashSet<Topic>();

            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            try
            {
                var enumerableSubscribersCache = await RegisteredSubscribersCache.GetEnumerableDictionaryAsync();

                foreach (var kvp in enumerableSubscribersCache)
                {
                    var topic = kvp.Key;
                    var subscribers = kvp.Value;

                    if (subscribers.Contains(subscriberServiceName))
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

        public async Task<bool> UnsubscribeFromTopic(Topic topic, string subscriberServiceName)
        {
            bool success;
            
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            try
            {
                short key = (short)topic;
                var result = await RegisteredSubscribersCache.TryGetValueAsync(key);

                if (!result.HasValue)
                {
                    return false;
                }
                
                var subscribers = result.Value;

                if (!subscribers.Contains(subscriberServiceName))
                {
                    return false;
                }

                subscribers.Remove(subscriberServiceName);
                await RegisteredSubscribersCache.SetAsync(key, subscribers);

                string debugMessage = $"{baseLogString} UnsubscribeFromTopic => {subscriberServiceName} SUCCESSFULLY unsubscribed from topic '{topic}'.";
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

        public async Task<bool> UnsubscribeFromTopics(IEnumerable<Topic> topics, string subscriberServiceName)
        {
            bool success;
            
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

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

                    if (!subscribers.Contains(subscriberServiceName))
                    {
                        continue;
                    }

                    subscribers.Remove(subscriberServiceName);
                    await RegisteredSubscribersCache.SetAsync(key, subscribers);

                    string debugMessage = $"{baseLogString} UnsubscribeFromTopics => {subscriberServiceName} SUCCESSFULLY unsubscribed from topic '{topic}'.";
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

        public async Task<bool> UnsubscribeFromAllTopics(string subscriberServiceName)
        {
            bool result;
            
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            try
            {
                var enumerableSubscribersCache = await RegisteredSubscribersCache.GetEnumerableDictionaryAsync();
                foreach (var topicKey in enumerableSubscribersCache.Keys)
                {
                    var subscribers = enumerableSubscribersCache[topicKey];

                    if (!subscribers.Contains(subscriberServiceName))
                    {
                        continue;
                    }

                    subscribers.Remove(subscriberServiceName);
                    await RegisteredSubscribersCache.SetAsync(topicKey, subscribers);

                    string debugMessage = $"{baseLogString} UnsubscribeFromAllTopics => {subscriberServiceName} SUCCESSFULLY unsubscribed from topic '{(Topic)topicKey}'.";
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

        public Task<bool> IsAlive()
        {
            return Task.Run(() => { return true; });
        }
    }
}

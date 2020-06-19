using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
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
        private readonly IReliableStateManager stateManager;
        private bool isSubscriberCacheInitialized;

        #region Private Properties
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
        #endregion Properties

        public RegisterSubscriberProvider(IReliableStateManager stateManager)
        {
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
        public async Task<bool> SubscribeToTopic(Topic topic, Uri subcriberUri, ServiceType serviceType)
        {
            while (!ReliableDictionariesInitialized)
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            bool result;

            try
            {
                short key = (short)topic;

                if (!RegisteredSubscribersCache.ContainsKey(key))
                {
                    await RegisteredSubscribersCache.SetAsync(key, new Dictionary<Uri, RegisteredSubscriber>());
                }

                var subscriber = new RegisteredSubscriber(subcriberUri, serviceType);

                if (!RegisteredSubscribersCache[key].ContainsKey(subcriberUri))
                {
                    RegisteredSubscribersCache[key].Add(subscriber.SubcriberUri, subscriber);
                }

                result = true;
            }
            catch (Exception e)
            {
                //TODO: log
                result = false;
            }

            return result;
        }

        public async Task<bool> SubscribeToTopics(IEnumerable<Topic> topics, Uri subcriberUri, ServiceType serviceType)
        {
            while (!ReliableDictionariesInitialized)
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            bool result;

            try
            {
                foreach (short key in topics)
                {
                    if (!RegisteredSubscribersCache.ContainsKey(key))
                    {
                        await RegisteredSubscribersCache.SetAsync(key, new Dictionary<Uri, RegisteredSubscriber>());
                    }

                    var subscriber = new RegisteredSubscriber(subcriberUri, serviceType);

                    if (!RegisteredSubscribersCache[key].ContainsKey(subcriberUri))
                    {
                        RegisteredSubscribersCache[key].Add(subscriber.SubcriberUri, subscriber);
                    }
                }

                result = true;
            }
            catch (Exception e)
            {
                //TODO: log
                result = false;
            }

            return result;
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

                foreach (var kvp in RegisteredSubscribersCache)
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
                //TODO: log
                throw e;
            }
        }    

        public async Task<bool> UnsubscribeFromTopic(Topic topic, Uri subcriberUri)
        {
            while (!ReliableDictionariesInitialized)
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            bool result;

            try
            {
                short key = (short)topic;

                if (!RegisteredSubscribersCache.ContainsKey(key))
                {
                    return false;
                }

                if (!RegisteredSubscribersCache[key].ContainsKey(subcriberUri))
                {
                    return false;
                }

                RegisteredSubscribersCache[key].Remove(subcriberUri);
                result = true;
            }
            catch (Exception e)
            {
                //TODO: log
                result = false;
            }

            return result;
        }

        public async Task<bool> UnsubscribeFromTopics(IEnumerable<Topic> topics, Uri subcriberUri)
        {
            while (!ReliableDictionariesInitialized)
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            bool result;

            try
            {
                foreach (short topic in topics)
                {
                    if (!RegisteredSubscribersCache.ContainsKey(topic))
                    {
                        continue;
                    }

                    if (!RegisteredSubscribersCache[topic].ContainsKey(subcriberUri))
                    {
                        continue;
                    }

                    RegisteredSubscribersCache[topic].Remove(subcriberUri);
                }

                result = true;
            }
            catch (Exception e)
            {
                //TODO: log
                result = false;
            }

            return result;
        }

        public async Task<bool> UnsubscribeFromAllTopics(Uri subcriberUri)
        {
            while (!ReliableDictionariesInitialized)
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            bool result;

            try
            {
                foreach (var subscribers in RegisteredSubscribersCache.Values)
                {
                    if (!subscribers.ContainsKey(subcriberUri))
                    {
                        continue;
                    }

                    subscribers.Remove(subcriberUri);
                }

                result = true;
            }
            catch (Exception e)
            {
                //TODO: log
                result = false;
            }

            return result;
        }
        #endregion IRegisterSubscriberContract
    }
}

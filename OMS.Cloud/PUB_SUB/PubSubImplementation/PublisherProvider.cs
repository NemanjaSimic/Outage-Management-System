using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.PubSub;
using OMS.Common.PubSubContracts;
using OMS.Common.PubSubContracts.Interfaces;
using OMS.Common.WcfClient.PubSub;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Runtime.InteropServices;
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
        #endregion Private Properties

        #region ReliableDictionary
        private bool isSubscriberCacheInitialized;
        private bool ReliableDictionariesInitialized
        {
            get { return true; }
        }

        private ReliableDictionaryAccess<short, HashSet<string>> registeredSubscribersCache;
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
            catch (FabricObjectClosedException)
            {
                Logger.LogDebug($"{baseLogString} OnStateManagerChangedHandler => FabricObjectClosedException. To be ignored.");
            }
            catch (COMException)
            {
                Logger.LogDebug($"{baseLogString} OnStateManagerChangedHandler => {typeof(COMException)}. To be ignored.");
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

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.RegisteredSubscribersCache}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
            }
        }
        #endregion

        public PublisherProvider(IReliableStateManager stateManager)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            this.isSubscriberCacheInitialized = false;

            this.stateManager = stateManager;
            //this.stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;
            registeredSubscribersCache = new ReliableDictionaryAccess<short, HashSet<string>>(stateManager, ReliableDictionaryNames.RegisteredSubscribersCache);
        }

        #region IPublisherContract
        public async Task<bool> Publish(IPublication publication, string publisherName)
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            bool success;

            try
            {
                ///Could be sole Task running in RunAsync, while reading from a queue...
                List<Task> tasks = new List<Task>();
                short key = (short)publication.Topic;

                var enumerableSubscribersCache = await RegisteredSubscribersCache.GetEnumerableDictionaryAsync();

                if (enumerableSubscribersCache.ContainsKey(key))
                {
                    var registeredSubscribers = enumerableSubscribersCache[key];

                    foreach (var subscriberName in registeredSubscribers)
                    {
                        var notifySubscriberClient = NotifySubscriberClient.CreateClient(subscriberName);
                        var task = notifySubscriberClient.Notify(publication.Message, publisherName);
                        tasks.Add(task);
                    }
                }
                ///////////

                Task.WaitAll(tasks.ToArray());
                success = true;

                Logger.LogInformation($"{baseLogString} Publish => SUCCESSFULL publication. PublicationType: {publication.GetType()}, MessageType: {publication.Message.GetType()}, Topic: {publication.Topic}");
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} Publish => Exception caught.";
                Logger.LogError(errorMessage, e);
                success = false;
            }

            return success;
        }

        public Task<bool> IsAlive()
        {
            return Task.Run(() => { return true; });
        }
        #endregion IPublisherContract
    }
}

using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Data.Notifications;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Fabric;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace OMS.Common.Cloud.ReliableCollectionHelpers
{
    public sealed class ReliableDictionaryAccess<TKey, TValue> : IReliableDictionary<TKey, TValue>,
                                                                 IEnumerable,
                                                                 IDisposable
                                                                 where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        private readonly string reliableDictionaryName;
        private readonly IReliableStateManager stateManager;
        private readonly ReliableStateManagerHelper reliableStateManagerHelper;
        
        private IReliableDictionary<TKey, TValue> reliableDictionary;

        #region Private Properties
        private IDictionary<TKey, TValue> localDictionary;
        private IDictionary<TKey, TValue> LocalDictionary
        {
            get { return localDictionary ?? (localDictionary = new Dictionary<TKey, TValue>()); }
        }
        
        private IDictionary<TKey, TValue> enumerableDictionary;
        private IDictionary<TKey, TValue> EnumerableDictionary
        {
            get { return enumerableDictionary ?? (enumerableDictionary = new Dictionary<TKey, TValue>()); }
        }
        #endregion Private Properties

        #region Static Members
        public static async Task<ReliableDictionaryAccess<TKey, TValue>> Create(IReliableStateManager stateManager, string reliableDictioanryName)
        {
            ReliableDictionaryAccess<TKey, TValue> reliableDictionaryAccess = new ReliableDictionaryAccess<TKey, TValue>(stateManager, reliableDictioanryName);
            await reliableDictionaryAccess.InitializeReliableDictionary(reliableDictioanryName);
            return reliableDictionaryAccess;   
        }

        public static async Task<ReliableDictionaryAccess<TKey, TValue>> Create(IReliableStateManager stateManager, IReliableDictionary<TKey, TValue> reliableDictionary)
        {
            ReliableDictionaryAccess<TKey, TValue> reliableDictionaryAccess = new ReliableDictionaryAccess<TKey, TValue>(stateManager, reliableDictionary);
            await reliableDictionaryAccess.InitializeReliableDictionary();
            return reliableDictionaryAccess;
        }
        #endregion Static Members

        #region Constructors
        internal ReliableDictionaryAccess(IReliableStateManager stateManager, string reliableDictioanryName)
        {
            this.stateManager = stateManager;
            this.reliableDictionaryName = reliableDictioanryName;
            this.reliableStateManagerHelper = new ReliableStateManagerHelper();
        }

        internal ReliableDictionaryAccess(IReliableStateManager stateManager, IReliableDictionary<TKey, TValue> reliableDictionary)
        {
            this.stateManager = stateManager;
            this.reliableDictionary = reliableDictionary;
            this.reliableDictionaryName = reliableDictionary.Name.OriginalString;
            this.reliableStateManagerHelper = new ReliableStateManagerHelper();
        }
        #endregion Constructors

        public async Task InitializeReliableDictionary(string reliableDictioanryName = "")
        {
            if (string.IsNullOrEmpty(reliableDictioanryName))
            {
                reliableDictioanryName = this.reliableDictionaryName;
            }

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                var result = await reliableStateManagerHelper.TryGetAsync<IReliableDictionary<TKey, TValue>>(this.stateManager, reliableDictioanryName);
                //var result = await this.stateManager.TryGetAsync<IReliableDictionary<TKey, TValue>>(reliableDictioanryName);

                if (result.HasValue)
                {
                    //cast is necessary
                    this.reliableDictionary = result.Value;
                    this.reliableDictionary.RebuildNotificationAsyncCallback = this.OnDictionaryRebuildNotificationHandlerAsync;
                    this.reliableDictionary.DictionaryChanged += this.OnDictionaryChangedHandler;
                    await tx.CommitAsync();
                }
                else
                {
                    string message = $"ReliableCollection Key: {reliableDictioanryName}, Type: {typeof(IReliableDictionary<TKey, TValue>)} was not initialized.";
                    throw new Exception(message);
                }
            }
        }

        public event EventHandler<NotifyDictionaryChangedEventArgs<TKey, TValue>> DictionaryChanged;
        
        public Dictionary<TKey, TValue> GetDataCopy()
        {
            return new Dictionary<TKey, TValue>(LocalDictionary);
        }

        #region NotificationHandlers
        private async Task OnDictionaryRebuildNotificationHandlerAsync(IReliableDictionary<TKey, TValue> origin, NotifyDictionaryRebuildEventArgs<TKey, TValue> e)
        {
            this.LocalDictionary.Clear();

            var enumerator = e.State.GetAsyncEnumerator();

            while (await enumerator.MoveNextAsync(CancellationToken.None))
            {
                this.LocalDictionary.Add(enumerator.Current.Key, enumerator.Current.Value);

                //IReliableState reliableState = reliableStatesEnumerator.Current;
                // // TODO: Add dictionary rebuild handler to reliableState as necessary
            }
        }

        private void OnDictionaryChangedHandler(object sender, NotifyDictionaryChangedEventArgs<TKey, TValue> e)
        {
            switch (e.Action)
            {
                case NotifyDictionaryChangedAction.Clear:
                    var clearEvent = e as NotifyDictionaryClearEventArgs<TKey, TValue>;
                    ProcessClearNotification(clearEvent);
                    return;

                case NotifyDictionaryChangedAction.Add:
                    var addEvent = e as NotifyDictionaryItemAddedEventArgs<TKey, TValue>;
                    ProcessAddNotification(addEvent);
                    return;

                case NotifyDictionaryChangedAction.Update:
                    var updateEvent = e as NotifyDictionaryItemUpdatedEventArgs<TKey, TValue>;
                    ProcessUpdateNotification(updateEvent);
                    return;

                case NotifyDictionaryChangedAction.Remove:
                    var deleteEvent = e as NotifyDictionaryItemRemovedEventArgs<TKey, TValue>;
                    ProcessRemoveNotification(deleteEvent);
                    return;

                case NotifyDictionaryChangedAction.Rebuild:
                    var rebuildEvent = e as NotifyDictionaryRebuildEventArgs<TKey, TValue>;
                    ProcessRebuildNotification(rebuildEvent);
                    return;

                default:
                    break;
            }
        }

        private void ProcessClearNotification(NotifyDictionaryClearEventArgs<TKey, TValue> e)
        {
            LocalDictionary.Clear();
        }

        private void ProcessAddNotification(NotifyDictionaryItemAddedEventArgs<TKey, TValue> e)
        {
            if(!LocalDictionary.ContainsKey(e.Key))
            {
                LocalDictionary.Add(e.Key, e.Value);
            }    
        }

        private void ProcessUpdateNotification(NotifyDictionaryItemUpdatedEventArgs<TKey, TValue> e)
        {
            LocalDictionary[e.Key] = e.Value;
        }

        private void ProcessRemoveNotification(NotifyDictionaryItemRemovedEventArgs<TKey, TValue> e)
        {
            if (LocalDictionary.ContainsKey(e.Key))
            {
                LocalDictionary.Remove(e.Key);
            }
        }

        private async void ProcessRebuildNotification(NotifyDictionaryRebuildEventArgs<TKey, TValue> e)
        {
            //TODO: test behavier; question: to use OnDictionaryChangedHandler or OnDictionaryRebuildNotificationHandlerAsync?
            //this.LocalDictionary.Clear();

            //var enumerator = e.State.GetAsyncEnumerator();

            //while (await enumerator.MoveNextAsync(CancellationToken.None))
            //{
            //    this.LocalDictionary.Add(enumerator.Current.Key, enumerator.Current.Value);
            //}
        }
        #endregion NotificationHandlers

        #region IReliableDictionary
        public Func<IReliableDictionary<TKey, TValue>, NotifyDictionaryRebuildEventArgs<TKey, TValue>, Task> RebuildNotificationAsyncCallback 
        { 
            set
            {
                if (reliableDictionary == null)
                {
                    InitializeReliableDictionary().Wait();
                }

                reliableDictionary.RebuildNotificationAsyncCallback = value;
            }
        }

        public Uri Name
        {
            get
            {
                if(reliableDictionary == null)
                {
                    InitializeReliableDictionary().Wait();
                }

                return reliableDictionary.Name;
            }
        }

        public async Task AddAsync(ITransaction tx, TKey key, TValue value)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            await reliableDictionary.AddAsync(tx, key, value);
        }

        public async Task AddAsync(ITransaction tx, TKey key, TValue value, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            await reliableDictionary.AddAsync(tx, key, value, timeout, cancellationToken);
        }

        public async Task<TValue> AddOrUpdateAsync(ITransaction tx, TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            return await reliableDictionary.AddOrUpdateAsync(tx, key, addValueFactory, updateValueFactory);
        }

        public async Task<TValue> AddOrUpdateAsync(ITransaction tx, TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            return await reliableDictionary.AddOrUpdateAsync(tx, key, addValue, updateValueFactory);
        }

        public async Task<TValue> AddOrUpdateAsync(ITransaction tx, TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            return await reliableDictionary.AddOrUpdateAsync(tx, key, addValueFactory, updateValueFactory, timeout, cancellationToken);
        }

        public async Task<TValue> AddOrUpdateAsync(ITransaction tx, TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            return await reliableDictionary.AddOrUpdateAsync(tx, key, addValue, updateValueFactory, timeout, cancellationToken);
        }

        public async Task ClearAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            await reliableDictionary.ClearAsync(timeout, cancellationToken);
        }

        public async Task ClearAsync()
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            await reliableDictionary.ClearAsync();
        }

        public async Task<bool> ContainsKeyAsync(ITransaction tx, TKey key)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            return await reliableDictionary.ContainsKeyAsync(tx, key);
        }

        public async Task<bool> ContainsKeyAsync(ITransaction tx, TKey key, LockMode lockMode)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            return await reliableDictionary.ContainsKeyAsync(tx, key, lockMode);
        }

        public async Task<bool> ContainsKeyAsync(ITransaction tx, TKey key, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            return await reliableDictionary.ContainsKeyAsync(tx, key, timeout, cancellationToken);
        }

        public async Task<bool> ContainsKeyAsync(ITransaction tx, TKey key, LockMode lockMode, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            return await reliableDictionary.ContainsKeyAsync(tx, key, lockMode, timeout, cancellationToken);
        }

        public async Task<IAsyncEnumerable<KeyValuePair<TKey, TValue>>> CreateEnumerableAsync(ITransaction txn)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            return await reliableDictionary.CreateEnumerableAsync(txn);
        }

        public async Task<IAsyncEnumerable<KeyValuePair<TKey, TValue>>> CreateEnumerableAsync(ITransaction txn, EnumerationMode enumerationMode)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            return await reliableDictionary.CreateEnumerableAsync(txn, enumerationMode);
        }

        public async Task<IAsyncEnumerable<KeyValuePair<TKey, TValue>>> CreateEnumerableAsync(ITransaction txn, Func<TKey, bool> filter, EnumerationMode enumerationMode)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            return await reliableDictionary.CreateEnumerableAsync(txn, filter, enumerationMode);
        }

        public async Task<long> GetCountAsync(ITransaction tx)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            return await reliableDictionary.GetCountAsync(tx);
        }

        public async Task<TValue> GetOrAddAsync(ITransaction tx, TKey key, Func<TKey, TValue> valueFactory)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            return await reliableDictionary.GetOrAddAsync(tx, key, valueFactory);
        }

        public async Task<TValue> GetOrAddAsync(ITransaction tx, TKey key, TValue value)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            return await reliableDictionary.GetOrAddAsync(tx, key, value);
        }

        public async Task<TValue> GetOrAddAsync(ITransaction tx, TKey key, Func<TKey, TValue> valueFactory, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            return await reliableDictionary.GetOrAddAsync(tx, key, valueFactory, timeout, cancellationToken);
        }

        public async Task<TValue> GetOrAddAsync(ITransaction tx, TKey key, TValue value, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            return await reliableDictionary.GetOrAddAsync(tx, key, value, timeout, cancellationToken);
        }

        public async Task SetAsync(ITransaction tx, TKey key, TValue value)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            await reliableDictionary.SetAsync(tx, key, value);
        }

        public async Task SetAsync(ITransaction tx, TKey key, TValue value, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            await reliableDictionary.SetAsync(tx, key, value, timeout, cancellationToken);
        }

        public async Task<bool> TryAddAsync(ITransaction tx, TKey key, TValue value)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            return await reliableDictionary.TryAddAsync(tx, key, value);
        }

        public async Task<bool> TryAddAsync(ITransaction tx, TKey key, TValue value, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            return await reliableDictionary.TryAddAsync(tx, key, value, timeout, cancellationToken);
        }

        public async Task<ConditionalValue<TValue>> TryGetValueAsync(ITransaction tx, TKey key)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            return await reliableDictionary.TryGetValueAsync(tx, key);
        }

        public async Task<ConditionalValue<TValue>> TryGetValueAsync(ITransaction tx, TKey key, LockMode lockMode)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            return await reliableDictionary.TryGetValueAsync(tx, key, lockMode);
        }

        public async Task<ConditionalValue<TValue>> TryGetValueAsync(ITransaction tx, TKey key, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            return await reliableDictionary.TryGetValueAsync(tx, key, timeout, cancellationToken);
        }

        public async Task<ConditionalValue<TValue>> TryGetValueAsync(ITransaction tx, TKey key, LockMode lockMode, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            return await reliableDictionary.TryGetValueAsync(tx, key, lockMode, timeout, cancellationToken);
        }

        public async Task<ConditionalValue<TValue>> TryRemoveAsync(ITransaction tx, TKey key)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            return await reliableDictionary.TryRemoveAsync(tx, key);
        }

        public async Task<ConditionalValue<TValue>> TryRemoveAsync(ITransaction tx, TKey key, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            return await reliableDictionary.TryRemoveAsync(tx, key, timeout, cancellationToken);
        }

        public async Task<bool> TryUpdateAsync(ITransaction tx, TKey key, TValue newValue, TValue comparisonValue)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            return await reliableDictionary.TryUpdateAsync(tx, key, newValue, comparisonValue);
        }

        public async Task<bool> TryUpdateAsync(ITransaction tx, TKey key, TValue newValue, TValue comparisonValue, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            return await reliableDictionary.TryUpdateAsync(tx, key, newValue, comparisonValue, timeout, cancellationToken);
        }
        #endregion IReliableDictionary

        #region IDictionary
        /// <summary>
        /// razmotriti upotreby Update() metode u slucaju wpf/win form aplikacija
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public TValue this[TKey key] 
        {
            get { return LocalDictionary[key]; }
            
            set
            {
                if (reliableDictionary == null)
                {
                    InitializeReliableDictionary().Wait(); //TODO: razmotriti 
                }

                using (ITransaction tx = stateManager.CreateTransaction())
                {
                    reliableDictionary.AddOrUpdateAsync(tx, key, k => value, (k, v) => value).Wait(); //TODO: razmotriti 
                    tx.CommitAsync().Wait(); //TODO: razmotriti 
                }
            }
        }

        public ICollection<TKey> Keys => LocalDictionary.Keys;

        public ICollection<TValue> Values => LocalDictionary.Values;

        public int Count => LocalDictionary.Count;

        public bool IsReadOnly => LocalDictionary.IsReadOnly;

        public async Task Add(TKey key, TValue value)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                await reliableDictionary.AddAsync(tx, key, value);
                await tx.CommitAsync();
            }
        }

        public async Task Add(KeyValuePair<TKey, TValue> item)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                await reliableDictionary.AddAsync(tx, item.Key, item.Value);
                await tx.CommitAsync();
            }
        }

        public async Task Clear()
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                await reliableDictionary.ClearAsync();
                await tx.CommitAsync();
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return LocalDictionary.Contains(item);
        }

        public bool ContainsKey(TKey key)
        {
            return LocalDictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            enumerableDictionary = new Dictionary<TKey, TValue>(LocalDictionary);
            return EnumerableDictionary.GetEnumerator();
        }

        public async Task<bool> Remove(TKey key)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            bool success;

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                var result = await reliableDictionary.TryRemoveAsync(tx, key);
                success = result.HasValue;

                if (success)
                {
                    await tx.CommitAsync();
                }
            }

            return success;
        }

        public async Task<bool> Remove(KeyValuePair<TKey, TValue> item)
        {
            return await Remove(item.Key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return LocalDictionary.TryGetValue(key, out value);
        }
        #endregion IDictionary

        #region IEnumerable
        IEnumerator IEnumerable.GetEnumerator()
        {
            enumerableDictionary = new Dictionary<TKey, TValue>(LocalDictionary);
            return EnumerableDictionary.GetEnumerator();
        }
        #endregion IEnumerable

        #region IDisposable
        public void Dispose()

        {
            throw new NotImplementedException();
        }
        #endregion IDisposable
    }
}

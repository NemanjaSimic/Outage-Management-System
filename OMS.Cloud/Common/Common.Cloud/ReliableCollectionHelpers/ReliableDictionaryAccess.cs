using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Data.Notifications;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OMS.Common.Cloud.ReliableCollectionHelpers
{
    public sealed class ReliableDictionaryAccess<TKey, TValue> : IReliableDictionary<TKey, TValue>,
                                                                 IEnumerable,
                                                                 IDisposable
                                                                 where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        public event EventHandler<NotifyDictionaryChangedEventArgs<TKey, TValue>> DictionaryChanged;
        
        private readonly IReliableStateManager stateManager;
        private readonly string reliableDictionaryName;
        
        private IReliableDictionary<TKey, TValue> reliableDictionary;
       
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

        public ReliableDictionaryAccess(IReliableStateManager stateManager, string reliableDictioanryName)
        {
            this.stateManager = stateManager;
            this.reliableDictionaryName = reliableDictioanryName;

            InitializeReliableDictionary(reliableDictioanryName);
        }

        public ReliableDictionaryAccess(IReliableStateManager stateManager, IReliableDictionary<TKey, TValue> reliableDictionary)
        {
            this.stateManager = stateManager;
            this.reliableDictionaryName = reliableDictionary.Name.OriginalString;
            this.reliableDictionary = reliableDictionary;
        }

        public async void InitializeReliableDictionary(string reliableDictioanryName = "")
        {
            if (string.IsNullOrEmpty(reliableDictioanryName))
            {
                reliableDictioanryName = this.reliableDictionaryName;
            }

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                this.reliableDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<TKey, TValue>>(tx, reliableDictioanryName);
                this.reliableDictionary.RebuildNotificationAsyncCallback = this.OnDictionaryRebuildNotificationHandlerAsync;
                this.reliableDictionary.DictionaryChanged += this.OnDictionaryChangedHandler;
                await tx.CommitAsync();
            }
        }

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
            LocalDictionary.Add(e.Key, e.Value);
        }

        private void ProcessUpdateNotification(NotifyDictionaryItemUpdatedEventArgs<TKey, TValue> e)
        {
            LocalDictionary[e.Key] = e.Value;
        }

        private void ProcessRemoveNotification(NotifyDictionaryItemRemovedEventArgs<TKey, TValue> e)
        {
            LocalDictionary.Remove(e.Key);
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
                    InitializeReliableDictionary();
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
                    InitializeReliableDictionary();
                }

                return reliableDictionary.Name;
            }
        }

        public Task AddAsync(ITransaction tx, TKey key, TValue value)
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
            }

            return reliableDictionary.AddAsync(tx, key, value);
        }

        public Task AddAsync(ITransaction tx, TKey key, TValue value, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
            }

            return reliableDictionary.AddAsync(tx, key, value, timeout, cancellationToken);
        }

        public Task<TValue> AddOrUpdateAsync(ITransaction tx, TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
            }

            return reliableDictionary.AddOrUpdateAsync(tx, key, addValueFactory, updateValueFactory);
        }

        public Task<TValue> AddOrUpdateAsync(ITransaction tx, TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
            }

            return reliableDictionary.AddOrUpdateAsync(tx, key, addValue, updateValueFactory);
        }

        public Task<TValue> AddOrUpdateAsync(ITransaction tx, TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
            }

            return reliableDictionary.AddOrUpdateAsync(tx, key, addValueFactory, updateValueFactory, timeout, cancellationToken);
        }

        public Task<TValue> AddOrUpdateAsync(ITransaction tx, TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
            }

            return reliableDictionary.AddOrUpdateAsync(tx, key, addValue, updateValueFactory, timeout, cancellationToken);
        }

        public Task ClearAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
            }

            return reliableDictionary.ClearAsync(timeout, cancellationToken);
        }

        public Task ClearAsync()
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
            }

            return reliableDictionary.ClearAsync();
        }

        public Task<bool> ContainsKeyAsync(ITransaction tx, TKey key)
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
            }

            return reliableDictionary.ContainsKeyAsync(tx, key);
        }

        public Task<bool> ContainsKeyAsync(ITransaction tx, TKey key, LockMode lockMode)
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
            }

            return reliableDictionary.ContainsKeyAsync(tx, key, lockMode);
        }

        public Task<bool> ContainsKeyAsync(ITransaction tx, TKey key, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
            }

            return reliableDictionary.ContainsKeyAsync(tx, key, timeout, cancellationToken);
        }

        public Task<bool> ContainsKeyAsync(ITransaction tx, TKey key, LockMode lockMode, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
            }

            return reliableDictionary.ContainsKeyAsync(tx, key, lockMode, timeout, cancellationToken);
        }

        public Task<IAsyncEnumerable<KeyValuePair<TKey, TValue>>> CreateEnumerableAsync(ITransaction txn)
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
            }

            return reliableDictionary.CreateEnumerableAsync(txn);
        }

        public Task<IAsyncEnumerable<KeyValuePair<TKey, TValue>>> CreateEnumerableAsync(ITransaction txn, EnumerationMode enumerationMode)
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
            }

            return reliableDictionary.CreateEnumerableAsync(txn, enumerationMode);
        }

        public Task<IAsyncEnumerable<KeyValuePair<TKey, TValue>>> CreateEnumerableAsync(ITransaction txn, Func<TKey, bool> filter, EnumerationMode enumerationMode)
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
            }

            return reliableDictionary.CreateEnumerableAsync(txn, filter, enumerationMode);
        }

        public Task<long> GetCountAsync(ITransaction tx)
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
            }

            return reliableDictionary.GetCountAsync(tx);
        }

        public Task<TValue> GetOrAddAsync(ITransaction tx, TKey key, Func<TKey, TValue> valueFactory)
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
            }

            return reliableDictionary.GetOrAddAsync(tx, key, valueFactory);
        }

        public Task<TValue> GetOrAddAsync(ITransaction tx, TKey key, TValue value)
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
            }

            return reliableDictionary.GetOrAddAsync(tx, key, value);
        }

        public Task<TValue> GetOrAddAsync(ITransaction tx, TKey key, Func<TKey, TValue> valueFactory, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
            }

            return reliableDictionary.GetOrAddAsync(tx, key, valueFactory, timeout, cancellationToken);
        }

        public Task<TValue> GetOrAddAsync(ITransaction tx, TKey key, TValue value, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
            }

            return reliableDictionary.GetOrAddAsync(tx, key, value, timeout, cancellationToken);
        }

        public Task SetAsync(ITransaction tx, TKey key, TValue value)
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
            }

            return reliableDictionary.SetAsync(tx, key, value);
        }

        public Task SetAsync(ITransaction tx, TKey key, TValue value, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
            }

            return reliableDictionary.SetAsync(tx, key, value, timeout, cancellationToken);
        }

        public Task<bool> TryAddAsync(ITransaction tx, TKey key, TValue value)
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
            }

            return reliableDictionary.TryAddAsync(tx, key, value);
        }

        public Task<bool> TryAddAsync(ITransaction tx, TKey key, TValue value, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
            }

            return reliableDictionary.TryAddAsync(tx, key, value, timeout, cancellationToken);
        }

        public Task<ConditionalValue<TValue>> TryGetValueAsync(ITransaction tx, TKey key)
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
            }

            return reliableDictionary.TryGetValueAsync(tx, key);
        }

        public Task<ConditionalValue<TValue>> TryGetValueAsync(ITransaction tx, TKey key, LockMode lockMode)
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
            }

            return reliableDictionary.TryGetValueAsync(tx, key, lockMode);
        }

        public Task<ConditionalValue<TValue>> TryGetValueAsync(ITransaction tx, TKey key, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
            }

            return reliableDictionary.TryGetValueAsync(tx, key, timeout, cancellationToken);
        }

        public Task<ConditionalValue<TValue>> TryGetValueAsync(ITransaction tx, TKey key, LockMode lockMode, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
            }

            return reliableDictionary.TryGetValueAsync(tx, key, lockMode, timeout, cancellationToken);
        }

        public Task<ConditionalValue<TValue>> TryRemoveAsync(ITransaction tx, TKey key)
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
            }

            return reliableDictionary.TryRemoveAsync(tx, key);
        }

        public Task<ConditionalValue<TValue>> TryRemoveAsync(ITransaction tx, TKey key, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
            }

            return reliableDictionary.TryRemoveAsync(tx, key, timeout, cancellationToken);
        }

        public Task<bool> TryUpdateAsync(ITransaction tx, TKey key, TValue newValue, TValue comparisonValue)
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
            }

            return reliableDictionary.TryUpdateAsync(tx, key, newValue, comparisonValue);
        }

        public Task<bool> TryUpdateAsync(ITransaction tx, TKey key, TValue newValue, TValue comparisonValue, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
            }

            return reliableDictionary.TryUpdateAsync(tx, key, newValue, comparisonValue, timeout, cancellationToken);
        }
        #endregion IReliableDictionary

        #region IDictionary
        public TValue this[TKey key] 
        {
            get { return LocalDictionary[key]; }
            
            set
            {
                if (reliableDictionary == null)
                {
                    InitializeReliableDictionary();
                }

                using (ITransaction tx = stateManager.CreateTransaction())
                {
                    reliableDictionary.AddOrUpdateAsync(tx, key, k => value, (k, v) => value);
                    tx.CommitAsync();
                }
            }
        }

        public ICollection<TKey> Keys => LocalDictionary.Keys;

        public ICollection<TValue> Values => LocalDictionary.Values;

        public int Count => LocalDictionary.Count;

        public bool IsReadOnly => LocalDictionary.IsReadOnly;

        public async void Add(TKey key, TValue value)
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
            }

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                await reliableDictionary.AddAsync(tx, key, value);
                await tx.CommitAsync();
            }
        }

        public async void Add(KeyValuePair<TKey, TValue> item)
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
            }

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                await reliableDictionary.AddAsync(tx, item.Key, item.Value);
                await tx.CommitAsync();
            }
        }

        public async void Clear()
        {
            if (reliableDictionary == null)
            {
                InitializeReliableDictionary();
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
                InitializeReliableDictionary();
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

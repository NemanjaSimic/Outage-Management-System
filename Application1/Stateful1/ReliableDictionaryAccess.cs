using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Data.Notifications;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Stateful1
{
    public sealed class ReliableDictionaryAccess<TKey, TValue> : IReliableDictionary<TKey, TValue>,
                                                                 IDisposable
                                                                 where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        private readonly string reliableDictionaryName;
        private readonly IReliableStateManager stateManager;
        private readonly ReliableStateManagerHelper reliableStateManagerHelper;

        private IReliableDictionary<TKey, TValue> reliableDictionary;

        #region Static Members
        //private static ICloudLogger logger;
        //private static ICloudLogger Logger
        //{
        //    get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        //}

        public static async Task<ReliableDictionaryAccess<TKey, TValue>> Create(IReliableStateManager stateManager, string reliableDictioanryName)
        {
            int numOfTriesLeft = 30;

            while (true)
            {
                try
                {
                    ReliableDictionaryAccess<TKey, TValue> reliableDictionaryAccess = new ReliableDictionaryAccess<TKey, TValue>(stateManager, reliableDictioanryName);
                    await reliableDictionaryAccess.InitializeReliableDictionary(reliableDictioanryName);
                    return reliableDictionaryAccess;
                }
                catch (Exception e)
                {
                    string message = $"Exception caught in {typeof(ReliableDictionaryAccess<TKey, TValue>)}.Create() method.";
                    //Logger.LogError(message, e);

                    if (numOfTriesLeft > 0)
                    {
                        await Task.Delay(1000);
                        numOfTriesLeft--;
                    }
                    else
                    {
                        throw e;
                    }
                }
            }
        }

        public static async Task<ReliableDictionaryAccess<TKey, TValue>> Create(IReliableStateManager stateManager, IReliableDictionary<TKey, TValue> reliableDictionary)
        {
            int numOfTriesLeft = 30;

            while (true)
            {
                try
                {
                    ReliableDictionaryAccess<TKey, TValue> reliableDictionaryAccess = new ReliableDictionaryAccess<TKey, TValue>(stateManager, reliableDictionary);
                    await reliableDictionaryAccess.InitializeReliableDictionary();
                    return reliableDictionaryAccess;
                }
                catch (Exception e)
                {
                    string message = $"Exception caught in {typeof(ReliableDictionaryAccess<TKey, TValue>)}.Create() method. NumOfTriesLeft: {numOfTriesLeft}";
                    //Logger.LogError(message, e);

                    if (numOfTriesLeft > 0)
                    {
                        await Task.Delay(1000);
                        numOfTriesLeft--;
                    }
                    else
                    {
                        throw e;
                    }
                }
            }
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

                if (result.HasValue)
                {
                    this.reliableDictionary = result.Value;
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

        public async Task<Dictionary<TKey, TValue>> GetDataCopyAsync()
        {
            Dictionary<TKey, TValue> copy = new Dictionary<TKey, TValue>();
            Microsoft.ServiceFabric.Data.IAsyncEnumerable<KeyValuePair<TKey, TValue>> asyncEnumerable;

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                asyncEnumerable = await reliableDictionary.CreateEnumerableAsync(tx);
            }

            var asyncEnumerator = asyncEnumerable.GetAsyncEnumerator();
            CancellationTokenSource tokenSource = new CancellationTokenSource();

            while (await asyncEnumerator.MoveNextAsync(tokenSource.Token))
            {
                var currentEntry = asyncEnumerator.Current;
                copy.Add(currentEntry.Key, currentEntry.Value);
            }

            return copy;
        }

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
                if (reliableDictionary == null)
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

        public async Task<Microsoft.ServiceFabric.Data.IAsyncEnumerable<KeyValuePair<TKey, TValue>>> CreateEnumerableAsync(ITransaction txn)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            return await reliableDictionary.CreateEnumerableAsync(txn);
        }

        public async Task<Microsoft.ServiceFabric.Data.IAsyncEnumerable<KeyValuePair<TKey, TValue>>> CreateEnumerableAsync(ITransaction txn, EnumerationMode enumerationMode)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            return await reliableDictionary.CreateEnumerableAsync(txn, enumerationMode);
        }

        public async Task<Microsoft.ServiceFabric.Data.IAsyncEnumerable<KeyValuePair<TKey, TValue>>> CreateEnumerableAsync(ITransaction txn, Func<TKey, bool> filter, EnumerationMode enumerationMode)
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

        #region Async Wrapper
        public async Task<bool> ContainsKeyAsync(TKey key)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                return await reliableDictionary.ContainsKeyAsync(tx, key);
            }
        }

        public async Task<long> GetCountAsync()
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                return await reliableDictionary.GetCountAsync(tx);
            }
        }

        public async Task<ConditionalValue<TValue>> TryGetValueAsync(TKey key)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                var result = await reliableDictionary.TryGetValueAsync(tx, key);

                if (result.HasValue)
                {
                    await tx.CommitAsync();
                }

                return result;
            }
        }

        public async Task<TValue> GetOrAddAsync(TKey key, TValue value)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                var result = await reliableDictionary.GetOrAddAsync(tx, key, value);
                await tx.CommitAsync();

                return result;
            }
        }

        public async Task SetAsync(TKey key, TValue value)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                await reliableDictionary.SetAsync(tx, key, value);
                await tx.CommitAsync();
            }
        }

        public async Task<bool> TryUpdateAsync(TKey key, TValue newValue, TValue comparisonValue)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                var result = await reliableDictionary.TryUpdateAsync(tx, key, newValue, comparisonValue);

                if (result)
                {
                    await tx.CommitAsync();
                }

                return result;
            }
        }

        public async Task<ConditionalValue<TValue>> TryRemoveAsync(TKey key)
        {
            if (reliableDictionary == null)
            {
                await InitializeReliableDictionary();
            }

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                var result = await reliableDictionary.TryRemoveAsync(tx, key);

                if (result.HasValue)
                {
                    await tx.CommitAsync();
                }

                return result;
            }
        }

        public async Task<Dictionary<TKey, TValue>> GetEnumerableDictionaryAsync()
        {
            var enumerableDictionary = await GetDataCopyAsync();
            return enumerableDictionary;
        }
        #endregion Async Wrapper

        #region IEnumerable
        public async Task<IEnumerator> GetEnumerator()
        {
            var enumerableDictionary = await GetDataCopyAsync();
            return enumerableDictionary.GetEnumerator();
        }
        #endregion IEnumerable

        #region IDisposable
        public void Dispose()

        {
            throw new NotImplementedException("ReliableDictionaryAccess.Dispose()");
        }
        #endregion IDisposable
    }
}

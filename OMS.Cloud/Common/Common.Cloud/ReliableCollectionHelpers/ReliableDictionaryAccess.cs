using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud.Logger;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OMS.Common.Cloud.ReliableCollectionHelpers
{
    public sealed class ReliableDictionaryAccess<TKey, TValue> where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        private readonly string reliableDictionaryName;
        private readonly IReliableStateManager stateManager;
        private readonly ReliableStateManagerHelper reliableStateManagerHelper;

        #region Static Members
        private static ICloudLogger logger;
        private static ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        public static async Task<ReliableDictionaryAccess<TKey, TValue>> Create(IReliableStateManager stateManager, string reliableDictioanryName)
        {
            int numOfTriesLeft = 30;

            while (true)
            {
                try
                {
                    var reliableDictionaryAccess = new ReliableDictionaryAccess<TKey, TValue>(stateManager, reliableDictioanryName);
                    _ = await reliableDictionaryAccess.GetReliableDictionary(reliableDictioanryName);
                    return reliableDictionaryAccess;
                }
                catch (Exception e)
                {
                    string message = $"Exception caught in {typeof(ReliableDictionaryAccess<TKey, TValue>)}.Create() method.";
                    Logger.LogError(message, e);

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
                    var reliableDictionaryAccess = new ReliableDictionaryAccess<TKey, TValue>(stateManager, reliableDictionary);
                    _ = await reliableDictionaryAccess.GetReliableDictionary();
                    return reliableDictionaryAccess;
                }
                catch (Exception e)
                {
                    string message = $"Exception caught in {typeof(ReliableDictionaryAccess<TKey, TValue>)}.Create() method. NumOfTriesLeft: {numOfTriesLeft}";
                    Logger.LogError(message, e);

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
            this.reliableDictionaryName = reliableDictionary.Name.OriginalString;
            this.reliableStateManagerHelper = new ReliableStateManagerHelper();
        }
        #endregion Constructors

        public async Task<IReliableDictionary<TKey, TValue>> GetReliableDictionary(string reliableDictioanryName = "")
        {
            try
            {
                if (string.IsNullOrEmpty(reliableDictioanryName))
                {
                    reliableDictioanryName = this.reliableDictionaryName;
                }

                using (ITransaction tx = stateManager.CreateTransaction())
                {
                    var result = await reliableStateManagerHelper.GetOrAddAsync<IReliableDictionary<TKey, TValue>>(this.stateManager, tx, reliableDictioanryName);

                    if (result != null)
                    {
                        return result;
                    }
                    else
                    {
                        string message = $"ReliableCollection Key: {reliableDictioanryName}, Type: {typeof(IReliableDictionary<TKey, TValue>)} was not initialized.";
                        throw new Exception(message);
                    }
                }
            }
            catch (Exception e)
            { 
                throw e;
            }
            
        }
        
        public async Task<Dictionary<TKey, TValue>> GetDataCopyAsync()
        {
            Dictionary<TKey, TValue> copy = new Dictionary<TKey, TValue>();
            Microsoft.ServiceFabric.Data.IAsyncEnumerable<KeyValuePair<TKey, TValue>> asyncEnumerable;

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                asyncEnumerable = await (await GetReliableDictionary()).CreateEnumerableAsync(tx);
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

        #region Async Wrapper
        public async Task<bool> ContainsKeyAsync(TKey key)
        {
            var reliableDictionary = await GetReliableDictionary();

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                return await reliableDictionary.ContainsKeyAsync(tx, key);
            }   
        }

        public async Task<long> GetCountAsync()
        {
            var reliableDictionary = await GetReliableDictionary();

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                return await reliableDictionary.GetCountAsync(tx);
            }
        }

        public async Task<ConditionalValue<TValue>> TryGetValueAsync(TKey key)
        {
            var reliableDictionary = await GetReliableDictionary();

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                var result = await reliableDictionary.TryGetValueAsync(tx, key);
                
                if(result.HasValue)
                {
                    await tx.CommitAsync();  
                }

                return result;
            }
        }

        public async Task<TValue> GetOrAddAsync(TKey key, TValue value)
        {
            var reliableDictionary = await GetReliableDictionary();

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                var result = await reliableDictionary.GetOrAddAsync(tx, key, value);
                await tx.CommitAsync();
                
                return result;
            }
        }

        public async Task SetAsync(TKey key, TValue value)
        {
            var reliableDictionary = await GetReliableDictionary();

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                await reliableDictionary.SetAsync(tx, key, value);
                await tx.CommitAsync();
            }
        }

        public async Task<bool> TryUpdateAsync(TKey key, TValue newValue, TValue comparisonValue)
        {
            var reliableDictionary = await GetReliableDictionary();

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                var result = await reliableDictionary.TryUpdateAsync(tx, key, newValue, comparisonValue);
                
                if(result)
                {
                    await tx.CommitAsync();
                }
                
                return result;
            }
        }

        public async Task<ConditionalValue<TValue>> TryRemoveAsync(TKey key)
        {
            var reliableDictionary = await GetReliableDictionary();

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

        public async Task ClearAsync()
        {
            var reliableDictionary = await GetReliableDictionary();
            await reliableDictionary.ClearAsync();
        }

        public async Task<Dictionary<TKey, TValue>> GetEnumerableDictionaryAsync()
        {
            var enumerableDictionary = await GetDataCopyAsync();
            return enumerableDictionary;
        }
        #endregion Async Wrapper
    }
}

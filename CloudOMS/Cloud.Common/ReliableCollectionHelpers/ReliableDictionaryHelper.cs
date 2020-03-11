using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System.Collections.Generic;
using System;
using System.Threading;

namespace Outage.Common.ReliableCollectionHelpers
{
    public static class ReliableDictionaryHelper
    {
        public static Dictionary<TKey, TValue> CopyToDictionary<TKey, TValue>(IReliableDictionary<TKey, TValue> reliableDictionary, IReliableStateManager stateManager) where TKey : IComparable<TKey>,
                                                                                                                                                                                     IEquatable<TKey>
        {
            Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();
            Microsoft.ServiceFabric.Data.IAsyncEnumerable<KeyValuePair<TKey, TValue>> asyncEnumerable;

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                asyncEnumerable = reliableDictionary.CreateEnumerableAsync(tx).Result;
            }
            
            var asyncEnumerator = asyncEnumerable.GetAsyncEnumerator();
            var currentEntry = asyncEnumerator.Current;
            dictionary.Add(currentEntry.Key, currentEntry.Value);

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            
            while (asyncEnumerator.MoveNextAsync(tokenSource.Token).Result)
            {
                currentEntry = asyncEnumerator.Current;
                dictionary.Add(currentEntry.Key, currentEntry.Value);
            }

            return dictionary;
        }

        public static bool TryCopyToReliableDictionary<TKey, TValue>(Dictionary<TKey, TValue> source, string targetKey, IReliableStateManager stateManager) where TKey : IComparable<TKey>,
                                                                                                                                                                         IEquatable<TKey>
        {
            try
            {
                var result = stateManager.TryGetAsync<IReliableDictionary<TKey, TValue>>(targetKey).Result;

                if (!result.HasValue)
                {
                    return false;
                }

                IReliableDictionary<TKey, TValue> reliableDictionary = result.Value;

                using (ITransaction tx = stateManager.CreateTransaction())
                {
                    foreach (var kvp in source)
                    {
                        reliableDictionary.AddOrUpdateAsync(tx, kvp.Key, kvp.Value, (key, value) => kvp.Value);
                    }

                    tx.CommitAsync();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return true;
        }

        public static bool TryCopyToReliableDictionary<TKey, TValue>(string sourceKey, string targetKey, IReliableStateManager stateManager) where TKey : IComparable<TKey>,
                                                                                                                                                          IEquatable<TKey>
        {
            try
            {
                var result = stateManager.TryGetAsync<IReliableDictionary<TKey, TValue>>(sourceKey).Result;

                if (!result.HasValue)
                {
                    return false;
                }

                IReliableDictionary<TKey, TValue> source = result.Value;

                result = stateManager.TryGetAsync<IReliableDictionary<TKey, TValue>>(targetKey).Result;

                if (!result.HasValue)
                {
                    return false;
                }

                IReliableDictionary<TKey, TValue> target = result.Value;

                using (ITransaction tx = stateManager.CreateTransaction())
                {
                    var asyncEnumerable = source.CreateEnumerableAsync(tx).Result;
                    var asyncEnumerator = asyncEnumerable.GetAsyncEnumerator();
                    
                    var currentEntry = asyncEnumerator.Current;
                    target.AddOrUpdateAsync(tx, currentEntry.Key, currentEntry.Value, (key, value) => currentEntry.Value);

                    CancellationTokenSource tokenSource = new CancellationTokenSource();

                    while (asyncEnumerator.MoveNextAsync(tokenSource.Token).Result)
                    {
                        currentEntry = asyncEnumerator.Current;
                        target.AddOrUpdateAsync(tx, currentEntry.Key, currentEntry.Value, (key, value) => currentEntry.Value);
                    }

                    tx.CommitAsync();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return true;
        }
    }
}

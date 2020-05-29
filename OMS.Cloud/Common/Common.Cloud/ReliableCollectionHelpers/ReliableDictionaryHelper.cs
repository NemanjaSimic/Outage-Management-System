using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OMS.Common.Cloud.ReliableCollectionHelpers
{
    public class ReliableDictionaryHelper
    {
        private readonly ReliableStateManagerHelper reliableStateManagerHelper;

        public ReliableDictionaryHelper()
        {
            this.reliableStateManagerHelper = new ReliableStateManagerHelper();
        }

        public async Task<ConditionalValue<Dictionary<TKey, TValue>>> TryCopyToDictionary<TKey, TValue>(string sourceKey, IReliableStateManager stateManager) where TKey : IComparable<TKey>, IEquatable<TKey>
        {
            var conditionalValue = await reliableStateManagerHelper.TryGetAsync<IReliableDictionary<TKey, TValue>>(stateManager, sourceKey);
            //var conditionalValue = await stateManager.TryGetAsync<IReliableDictionary<TKey, TValue>>(sourceKey);

            if (!conditionalValue.HasValue)
            {
                return new ConditionalValue<Dictionary<TKey, TValue>>(false, null);
            }

            IReliableDictionary<TKey, TValue> source = conditionalValue.Value;
            return await TryCopyToDictionary<TKey, TValue>(source, stateManager);
        }

        public async Task<ConditionalValue<Dictionary<TKey, TValue>>> TryCopyToDictionary<TKey, TValue>(IReliableDictionary<TKey, TValue> source, IReliableStateManager stateManager) where TKey : IComparable<TKey>, IEquatable<TKey>
        {
            Dictionary<TKey, TValue> target = new Dictionary<TKey, TValue>();

            IAsyncEnumerable<KeyValuePair<TKey, TValue>> asyncEnumerable;

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                asyncEnumerable = await source.CreateEnumerableAsync(tx);
            }

            var asyncEnumerator = asyncEnumerable.GetAsyncEnumerator();
            var currentEntry = asyncEnumerator.Current;
            target.Add(currentEntry.Key, currentEntry.Value);

            CancellationTokenSource tokenSource = new CancellationTokenSource();

            while(await asyncEnumerator.MoveNextAsync(tokenSource.Token))
            {
                currentEntry = asyncEnumerator.Current;
                target.Add(currentEntry.Key, currentEntry.Value);
            }

            return new ConditionalValue<Dictionary<TKey, TValue>>(true, target);
        }

        public async Task<ConditionalValue<IReliableDictionary<TKey, TValue>>> TryCopyToReliableDictionary<TKey, TValue>(Dictionary<TKey, TValue> source, string targetKey, IReliableStateManager stateManager) where TKey : IComparable<TKey>, IEquatable<TKey>
        {
            var conditionalValue = await reliableStateManagerHelper.TryGetAsync<IReliableDictionary<TKey, TValue>>(stateManager, targetKey);
            //var conditionalValue = await stateManager.TryGetAsync<IReliableDictionary<TKey, TValue>>(targetKey);

            if (!conditionalValue.HasValue)
            {
                return new ConditionalValue<IReliableDictionary<TKey, TValue>>(false, null);
            }

            IReliableDictionary<TKey, TValue> reliableDictionary = conditionalValue.Value;

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                foreach (var kvp in source)
                {
                    await reliableDictionary.AddOrUpdateAsync(tx, kvp.Key, kvp.Value, (key, value) => kvp.Value);
                }

                await tx.CommitAsync();
            }

            return new ConditionalValue<IReliableDictionary<TKey, TValue>>(true, reliableDictionary);
        }

        public async Task<ConditionalValue<IReliableDictionary<TKey, TValue>>> TryCopyToReliableDictionary<TKey, TValue>(string sourceKey, string targetKey, IReliableStateManager stateManager) where TKey : IComparable<TKey>, IEquatable<TKey>
        {
            var conditionalValue = await reliableStateManagerHelper.TryGetAsync<IReliableDictionary<TKey, TValue>>(stateManager, sourceKey);
            //var conditionalValue = await stateManager.TryGetAsync<IReliableDictionary<TKey, TValue>>(sourceKey);
            if (!conditionalValue.HasValue)
            {
                return new ConditionalValue<IReliableDictionary<TKey, TValue>>(false, null);
            }

            IReliableDictionary<TKey, TValue> source = conditionalValue.Value;

            conditionalValue = await reliableStateManagerHelper.TryGetAsync<IReliableDictionary<TKey, TValue>>(stateManager, targetKey);
            //conditionalValue = await stateManager.TryGetAsync<IReliableDictionary<TKey, TValue>>(targetKey);
            if (!conditionalValue.HasValue)
            {
                return new ConditionalValue<IReliableDictionary<TKey, TValue>>(false, null);
            }

            IReliableDictionary<TKey, TValue> target = conditionalValue.Value;

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                var asyncEnumerable = await source.CreateEnumerableAsync(tx);
                var asyncEnumerator = asyncEnumerable.GetAsyncEnumerator();

                var currentEntry = asyncEnumerator.Current;
                await target.AddOrUpdateAsync(tx, currentEntry.Key, currentEntry.Value, (key, value) => currentEntry.Value);

                CancellationTokenSource tokenSource = new CancellationTokenSource();

                while(await asyncEnumerator.MoveNextAsync(tokenSource.Token))
                {
                    currentEntry = asyncEnumerator.Current;
                    await target.AddOrUpdateAsync(tx, currentEntry.Key, currentEntry.Value, (key, value) => currentEntry.Value);
                }

                await tx.CommitAsync();
            }

            return new ConditionalValue<IReliableDictionary<TKey, TValue>>(true, target);
        }
    }
}

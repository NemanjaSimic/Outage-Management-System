using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Common.Cloud.ReliableCollectionHelpers
{
    enum OperatinType
    {
        Insert = 1,
        Update,
        Remove,
    }

    public sealed class ReliableDictionaryUnitOfWork<TKey, TValue> : IDictionary<TKey, TValue>, IDisposable where TKey : IComparable<TKey>, IEquatable<TKey>
    {
        private readonly string reliableDictionaryKey;
        private readonly IReliableStateManager stateManager;
        private readonly ITransaction transaction;

        //TODO: instead of localDictionary
        private Dictionary<TKey, TValue> localDictionary;
        private Dictionary<TKey, OperatinType> operationLedger;

        public ReliableDictionaryUnitOfWork(string key, IReliableStateManager stateManager)
        {
            this.reliableDictionaryKey = key;
            this.stateManager = stateManager;
            this.transaction = stateManager.CreateTransaction();

            localDictionary = new Dictionary<TKey, TValue>();
            operationLedger = new Dictionary<TKey, OperatinType>();

            var result = stateManager.TryGetAsync<IReliableDictionary<TKey, TValue>>(key).Result;

            if (!result.HasValue)
            {
                throw new KeyNotFoundException();
            }

            if(!ReliableDictionaryHelper.TryCopyToDictionary(result.Value, stateManager, out localDictionary))
            {
                throw new Exception("ReliableDictionaryUnitOfWork::ctor => TryCopyToDictionary failed.");
            }
        }

        public void Dispose()
        {
            transaction.Dispose();
        }

        public async Task<bool> Complete()
        {
            var result = await stateManager.TryGetAsync<IReliableDictionary<TKey, TValue>>(reliableDictionaryKey);

            if(!result.HasValue)
            {
                return false;
            }

            IReliableDictionary<TKey, TValue> reliableDictionary = result.Value;
            List<Task> tasks = new List<Task>(operationLedger.Count);

            foreach (TKey key in operationLedger.Keys)
            {
                if (operationLedger[key] == OperatinType.Insert)
                {
                    Task task = Task.Run(() => reliableDictionary.TryAddAsync(transaction, key, localDictionary[key]));
                    tasks.Add(task);
                }
                else if (operationLedger[key] == OperatinType.Update)
                {
                    Task task = Task.Run(() =>
                    {
                        var operationResult = reliableDictionary.TryGetValueAsync(transaction, key).Result;
                        
                        if(operationResult.HasValue)
                        {
                            reliableDictionary.TryUpdateAsync(transaction, key, localDictionary[key], operationResult.Value);
                        }
                    
                    });
                    tasks.Add(task);
                }
                else if(operationLedger[key] == OperatinType.Remove)
                {
                    Task task = Task.Run(() => reliableDictionary.TryRemoveAsync(transaction, key));
                    tasks.Add(task);
                }
            }

            await Task.WhenAll(tasks);
            await transaction.CommitAsync();
            
            return true;
        }

        public ICollection<TKey> Keys => localDictionary.Keys;

        public ICollection<TValue> Values => localDictionary.Values;

        public int Count => localDictionary.Count;

        public bool IsReadOnly => false;

        public TValue this[TKey key]
        {
            get
            {
                return localDictionary[key];
            }

            set
            {
                OperatinType operatinType = localDictionary.ContainsKey(key) ? OperatinType.Update : OperatinType.Insert;
                localDictionary[key] = value;

                if(operationLedger.ContainsKey(key))
                {
                    operationLedger[key] = operatinType;
                }
                else
                {
                    operationLedger.Add(key, operatinType);
                }
            }
        }

        public bool ContainsKey(TKey key)
        {
            return localDictionary.ContainsKey(key);
        }

        public void Add(TKey key, TValue value)
        {
            localDictionary[key] = value;
            operationLedger.Add(key, OperatinType.Insert);
        }

        public bool Remove(TKey key)
        {
            if(localDictionary.ContainsKey(key))
            {
                operationLedger[key] = OperatinType.Remove;
            }

            return localDictionary.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return localDictionary.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            localDictionary.Keys.ToList().ForEach(key => operationLedger[key] = OperatinType.Remove);
            localDictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return localDictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return localDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return localDictionary.GetEnumerator();
        }
    }
}

using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using OMS.Common.Cloud.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OMS.Common.Cloud.ReliableCollectionHelpers
{
    public sealed class ReliableQueueAccess<TValue> : IReliableQueue<TValue>
    {
        private readonly string reliableQueueName;
        private readonly IReliableStateManager stateManager;
        private readonly ReliableStateManagerHelper reliableStateManagerHelper;

        private IReliableQueue<TValue> reliableQueue;

        #region Static Members
        private static ICloudLogger logger;
        private static ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        public static async Task<ReliableQueueAccess<TValue>> Create(IReliableStateManager stateManager, string reliableQueueName)
        {
            int numOfTriesLeft = 30;

            while (true)
            {
                try
                {
                    var reliableQueueAccess = new ReliableQueueAccess<TValue>(stateManager, reliableQueueName);
                    await reliableQueueAccess.InitializeReliableQueue(reliableQueueName);
                    return reliableQueueAccess;
                }
                catch (Exception e)
                {
                    string message = $"Exception caught in {typeof(ReliableQueueAccess<TValue>)}.Create() method.";
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
        internal ReliableQueueAccess(IReliableStateManager stateManager, string reliableDictioanryName)
        {
            this.stateManager = stateManager;
            this.reliableQueueName = reliableDictioanryName;
            this.reliableStateManagerHelper = new ReliableStateManagerHelper();
        }
        #endregion Constructors

        public async Task InitializeReliableQueue(string reliableQueueName = "")
        {
            if (string.IsNullOrEmpty(reliableQueueName))
            {
                reliableQueueName = this.reliableQueueName;
            }

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                var result = await reliableStateManagerHelper.TryGetAsync<IReliableQueue<TValue>>(this.stateManager, reliableQueueName);

                if (result.HasValue)
                {
                    this.reliableQueue = result.Value;
                    await tx.CommitAsync();
                }
                else
                {
                    string message = $"ReliableCollection Key: {reliableQueueName}, Type: {typeof(IReliableQueue<TValue>)} was not initialized.";
                    throw new Exception(message);
                }
            }
        }

        #region IReliableQueue
        public Uri Name 
        {
            get { return reliableQueue.Name; }
        }

        public async Task ClearAsync()
        {
            await reliableQueue.ClearAsync();
        }

        public async Task<IAsyncEnumerable<TValue>> CreateEnumerableAsync(ITransaction tx)
        {
            return await reliableQueue.CreateEnumerableAsync(tx);
        }

        public async Task EnqueueAsync(ITransaction tx, TValue item)
        {
            await reliableQueue.EnqueueAsync(tx, item);
        }

        public async Task EnqueueAsync(ITransaction tx, TValue item, TimeSpan timeout, CancellationToken cancellationToken)
        {
            await reliableQueue.EnqueueAsync(tx, item, timeout, cancellationToken);
        }

        public async Task<long> GetCountAsync(ITransaction tx)
        {
            return await reliableQueue.GetCountAsync(tx);
        }

        public async Task<ConditionalValue<TValue>> TryDequeueAsync(ITransaction tx)
        {
            return await reliableQueue.TryDequeueAsync(tx);
        }

        public async Task<ConditionalValue<TValue>> TryDequeueAsync(ITransaction tx, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return await reliableQueue.TryDequeueAsync(tx, timeout, cancellationToken);
        }

        public async Task<ConditionalValue<TValue>> TryPeekAsync(ITransaction tx)
        {
            return await reliableQueue.TryPeekAsync(tx);
        }

        public async Task<ConditionalValue<TValue>> TryPeekAsync(ITransaction tx, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return await reliableQueue.TryPeekAsync(tx, timeout, cancellationToken);
        }

        public async Task<ConditionalValue<TValue>> TryPeekAsync(ITransaction tx, LockMode lockMode)
        {
            return await reliableQueue.TryPeekAsync(tx, lockMode);
        }

        public async Task<ConditionalValue<TValue>> TryPeekAsync(ITransaction tx, LockMode lockMode, TimeSpan timeout, CancellationToken cancellationToken)
        {
            return await reliableQueue.TryPeekAsync(tx, lockMode, timeout, cancellationToken);
        }
        #endregion IReliableQueue

        #region Async Wrapper
        public async Task<IAsyncEnumerable<TValue>> CreateEnumerableAsync()
        {
            if (reliableQueue == null)
            {
                await InitializeReliableQueue();
            }

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                return await reliableQueue.CreateEnumerableAsync(tx);
            }
        }

        public async Task EnqueueAsync(TValue item)
        {
            if (reliableQueue == null)
            {
                await InitializeReliableQueue();
            }

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                await reliableQueue.EnqueueAsync(tx, item);
            }
        }

        public async Task<long> GetCountAsync()
        {
            if (reliableQueue == null)
            {
                await InitializeReliableQueue();
            }

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                return await reliableQueue.GetCountAsync(tx);
            }
        }

        public async Task<ConditionalValue<TValue>> TryDequeueAsync()
        {
            if (reliableQueue == null)
            {
                await InitializeReliableQueue();
            }

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                return await reliableQueue.TryDequeueAsync(tx);
            }   
        }

        public async Task<ConditionalValue<TValue>> TryPeekAsync()
        {
            if (reliableQueue == null)
            {
                await InitializeReliableQueue();
            }

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                return await reliableQueue.TryPeekAsync(tx);
            }
        }
        #endregion Async Wrapper
    }
}

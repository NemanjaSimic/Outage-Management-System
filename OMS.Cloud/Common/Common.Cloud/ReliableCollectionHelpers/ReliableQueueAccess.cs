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
    public sealed class ReliableQueueAccess<TValue> : IReliableConcurrentQueue<TValue>
    {
        private readonly string reliableQueueName;
        private readonly IReliableStateManager stateManager;
        private readonly ReliableStateManagerHelper reliableStateManagerHelper;

        private IReliableConcurrentQueue<TValue> reliableConcurrentQueue;

        #region Static Members
        private static ICloudLogger logger;
        private static ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

		public long Count => throw new NotImplementedException();

		public Uri Name => throw new NotImplementedException();

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
                var result = await reliableStateManagerHelper.TryGetAsync<IReliableConcurrentQueue<TValue>>(this.stateManager, reliableQueueName);

                if (result.HasValue)
                {
                    this.reliableConcurrentQueue = result.Value;
                    await tx.CommitAsync();
                }
                else
                {
                    string message = $"ReliableCollection Key: {reliableQueueName}, Type: {typeof(IReliableConcurrentQueue<TValue>)} was not initialized.";
                    throw new Exception(message);
                }
            }
        }

        #region IReliableConcurentQueue
        public async Task EnqueueAsync(ITransaction tx, TValue value, CancellationToken cancellationToken = default, TimeSpan? timeout = null)
        {
            if (reliableConcurrentQueue == null)
            {
                await InitializeReliableQueue();
            }

            await reliableConcurrentQueue.EnqueueAsync(tx, value, cancellationToken, timeout);
        }

        public async Task<ConditionalValue<TValue>> TryDequeueAsync(ITransaction tx, CancellationToken cancellationToken = default, TimeSpan? timeout = null)
        {
            if (reliableConcurrentQueue == null)
            {
                await InitializeReliableQueue();
            }

            return await reliableConcurrentQueue.TryDequeueAsync(tx, cancellationToken, timeout);
        }
        #endregion IReliableConcurentQueue

        #region Async Wrapper
        public async Task ClearAsync()
		{
            if (reliableConcurrentQueue == null)
            {
                await InitializeReliableQueue();
            }

            while ((await TryDequeueAsync()).HasValue) ;
        }

        public async Task EnqueueAsync(TValue item)
        {
            if (reliableConcurrentQueue == null)
            {
                await InitializeReliableQueue();
            }

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                await reliableConcurrentQueue.EnqueueAsync(tx, item);
                await tx.CommitAsync();
            }
        }

        public async Task<long> GetCountAsync()
        {
            if (reliableConcurrentQueue == null)
            {
                await InitializeReliableQueue();
            }
            return reliableConcurrentQueue.Count;   
        }

        public async Task<ConditionalValue<TValue>> TryDequeueAsync()
        {
            if (reliableConcurrentQueue == null)
            {
                await InitializeReliableQueue();
            }

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                ConditionalValue<TValue> result = await reliableConcurrentQueue.TryDequeueAsync(tx);

                await tx.CommitAsync();

                return result;
            }   
        }
		#endregion Async Wrapper
	}
}

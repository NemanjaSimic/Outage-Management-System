using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using OMS.Common.Cloud.Logger;
using System;
using System.Threading.Tasks;

namespace OMS.Common.Cloud.ReliableCollectionHelpers
{
    public sealed class ReliableQueueAccess<TValue>
    {
        private readonly string reliableQueueName;
        private readonly IReliableStateManager stateManager;
        private readonly ReliableStateManagerHelper reliableStateManagerHelper;

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
                    _ = await reliableQueueAccess.GetReliableQueue(reliableQueueName);
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
        public ReliableQueueAccess(IReliableStateManager stateManager, string reliableDictioanryName)
        {
            this.stateManager = stateManager;
            this.reliableQueueName = reliableDictioanryName;
            this.reliableStateManagerHelper = new ReliableStateManagerHelper();
        }
        #endregion Constructors

        public async Task<IReliableConcurrentQueue<TValue>> GetReliableQueue(string reliableQueueName = "")
        {
            if (string.IsNullOrEmpty(reliableQueueName))
            {
                reliableQueueName = this.reliableQueueName;
            }

            try
            {
                var result = await reliableStateManagerHelper.TryGetAsync<IReliableConcurrentQueue<TValue>>(this.stateManager, reliableQueueName);

                if (result.HasValue)
                {
                    return result.Value;
                }
                else
                {
                    string message = $"ReliableCollection Key: {reliableQueueName}, Type: {typeof(IReliableConcurrentQueue<TValue>)} was not initialized.";
                    throw new Exception(message);
                }
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        #region Async Wrapper
        public async Task ClearAsync()
		{
            while ((await TryDequeueAsync()).HasValue);
        }

        public async Task EnqueueAsync(TValue item)
        {
            var reliableConcurrentQueue = await GetReliableQueue();

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                await reliableConcurrentQueue.EnqueueAsync(tx, item);
                await tx.CommitAsync();
            }
        }

        public async Task<long> GetCountAsync()
        {
            var reliableConcurrentQueue = await GetReliableQueue();
            return reliableConcurrentQueue.Count;   
        }

        public async Task<ConditionalValue<TValue>> TryDequeueAsync()
        {
            var reliableConcurrentQueue = await GetReliableQueue();

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

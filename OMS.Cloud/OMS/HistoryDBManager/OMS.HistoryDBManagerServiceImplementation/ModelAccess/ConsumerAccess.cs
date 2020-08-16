using Common.OMS.OutageDatabaseModel;
using Common.OmsContracts.ModelAccess;
using OMS.Common.Cloud.Logger;
using OutageDatabase.Repository;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OMS.HistoryDBManagerImplementation.ModelAccess
{
    public class ConsumerAccess : IConsumerAccessContract
	{
		private readonly string baseLogString;

		#region Private Properties
		private ICloudLogger logger;
		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}
		#endregion Private Properties

		public ConsumerAccess()
		{
			this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
			Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");
		}

		#region IConsumerAccessContract
		public Task<Consumer> AddConsumer(Consumer consumer)
		{
			return Task.Run(() =>
			{
				//todo: razmisliti o ConditonalValue<Consumer>
				Consumer consumerDb = null;

				using (var unitOfWork = new UnitOfWork())
				{
					try
					{
						consumerDb = unitOfWork.ConsumerRepository.Add(consumer);
						unitOfWork.Complete();
					}
					catch (Exception e)
					{
						string message = $"{baseLogString} AddConsumer => Exception: {e.Message}";
						Logger.LogError(message, e);
					}
				}

				return consumerDb;
			});
		}

		public Task<IEnumerable<Consumer>> FindConsumer(Expression<Func<Consumer, bool>> predicate)
		{
			return Task.Run(() =>
			{
				IEnumerable<Consumer> consumers = new List<Consumer>();

				using (var unitOfWork = new UnitOfWork())
				{
					try
					{
						consumers = unitOfWork.ConsumerRepository.Find(predicate);
					}
					catch (Exception e)
					{
						string message = $"{baseLogString} FindConsumer => Exception: {e.Message}";
						Logger.LogError(message, e);
					}
				}

				return consumers;
			});
		}

		public Task<IEnumerable<Consumer>> GetAllConsumers()
		{
			return Task.Run(() =>
			{
				IEnumerable<Consumer> consumers = new List<Consumer>();

				using (var unitOfWork = new UnitOfWork())
				{
					try
					{
						consumers = unitOfWork.ConsumerRepository.GetAll();
					}
					catch (Exception e)
					{
						string message = $"{baseLogString} GetAllConsumers => Exception: {e.Message}";
						Logger.LogError(message, e);
					}
				}

				return consumers;
			});
		}

		public Task<Consumer> GetConsumer(long gid)
		{
			return Task.Run(() =>
			{
				//todo: razmisliti o ConditonalValue<Consumer>
				Consumer consumer = null;

				using (var unitOfWork = new UnitOfWork())
				{
					try
					{
						consumer = unitOfWork.ConsumerRepository.Get(gid);
					}
					catch (Exception e)
					{
						string message = $"{baseLogString} GetConsumer => Exception: {e.Message}";
						Logger.LogError(message, e);
					}
				}

				return consumer;
			});
		}

		public Task RemoveAllConsumers()
		{
			return Task.Run(() =>
			{
				using (var unitOfWork = new UnitOfWork())
				{
					try
					{
						unitOfWork.ConsumerRepository.RemoveAll();
						unitOfWork.Complete();
					}
					catch (Exception e)
					{
						string message = $"{baseLogString} RemoveAllConsumers => Exception: {e.Message}";
						Logger.LogError(message, e);
					}
				}
			});
		}

		public Task RemoveConsumer(Consumer consumer)
		{
			return Task.Run(() => 
			{
				using (var unitOfWork = new UnitOfWork())
				{
					try
					{
						unitOfWork.ConsumerRepository.Remove(consumer);
						unitOfWork.Complete();
					}
					catch (Exception e)
					{
						string message = $"{baseLogString} RemoveConsumer => Exception: {e.Message}";
						Logger.LogError(message, e);
					}
				}
			});
		}

		public Task UpdateConsumer(Consumer consumer)
		{
			return Task.Run(() =>
            {
				using(var unitOfWork = new UnitOfWork())
                {
					try
					{
						unitOfWork.ConsumerRepository.Update(consumer);
						unitOfWork.Complete();
					}
					catch (Exception e)
					{
						string message = $"{baseLogString} UpdateConsumer => Exception: {e.Message}";
						Logger.LogError(message, e);
					}
                }
			});
		}

		public Task<bool> IsAlive()
		{
			return Task.Run(() => true);
		}
		#endregion IConsumerAccessContract
	}
}

using Common.OMS.OutageDatabaseModel;
using Common.OmsContracts.ModelAccess;
using OutageDatabase.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OMS.HistoryDBManagerServiceImplementation.ModelAccess
{
	public class ConsumerAccess : IConsumerAccessContract
	{
		private UnitOfWork dbContext;

		public ConsumerAccess()
		{
			this.dbContext = new UnitOfWork();
		}
		public Task<Consumer> AddConsumer(Consumer consumer)
		{
			return new Task<Consumer>(() =>
			{
				Consumer consumerDb = dbContext.ConsumerRepository.Add(consumer);
				dbContext.Complete();
				return consumerDb;
			});
		}

		public Task<IEnumerable<Consumer>> FindConsumer(Expression<Func<Consumer, bool>> predicate)
		{
			return new Task<IEnumerable<Consumer>>(() => dbContext.ConsumerRepository.Find(predicate));
		}

		public Task<IEnumerable<Consumer>> GetAllConsumers()
		{
			return new Task<IEnumerable<Consumer>>(() => dbContext.ConsumerRepository.GetAll());
		}

		public Task<Consumer> GetConsumer(long gid)
		{
			return new Task<Consumer>(() => dbContext.ConsumerRepository.Get(gid));
		}

		public Task RemoveAllConsumers()
		{
			return new Task(() =>
			{
				dbContext.ConsumerRepository.RemoveAll();
				dbContext.Complete();
			});
		}

		public Task RemoveConsumer(Consumer consumer)
		{
			return new Task(() => 
			{
				dbContext.ConsumerRepository.Remove(consumer);
				dbContext.Complete();
			});
		}

		public Task UpdateConsumer(Consumer consumer)
		{
			return new Task(() =>
			{
				dbContext.ConsumerRepository.Update(consumer);
				dbContext.Complete();
			});
		}
	}
}

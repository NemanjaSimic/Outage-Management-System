using Common.OMS.OutageDatabaseModel;
using Common.OmsContracts.ModelAccess;
using OutageDatabase.Repository;
using System;
using System.Collections.Generic;
using System.Fabric.Repair;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace OMS.HistoryDBManagerServiceImplementation.ModelAccess
{
	public class OutageModelAccess : IOutageAccessContract
	{
		private UnitOfWork dbContext;
		public OutageModelAccess()
		{
			this.dbContext = new UnitOfWork();
		}

		public Task<OutageEntity> AddOutage(OutageEntity outage)
		{
			return new Task<OutageEntity>(() =>
			{
				OutageEntity outageEntityDb = dbContext.OutageRepository.Add(outage);
				dbContext.Complete();
				return outageEntityDb;
			});
		}

		public Task<IEnumerable<OutageEntity>> FindOutage(Expression<Func<OutageEntity, bool>> predicate)
		{
			return new Task<IEnumerable<OutageEntity>>(() => dbContext.OutageRepository.Find(predicate));
		}

		public Task<IEnumerable<OutageEntity>> GetAllActiveOutages()
		{
			return new Task<IEnumerable<OutageEntity>>(() => dbContext.OutageRepository.GetAllActive());
		}

		public Task<IEnumerable<OutageEntity>> GetAllArchivedOutages()
		{
			return new Task<IEnumerable<OutageEntity>>(() => dbContext.OutageRepository.GetAllArchived());
		}

		public Task<IEnumerable<OutageEntity>> GetAllOutages()
		{
			return new Task<IEnumerable<OutageEntity>>(() => dbContext.OutageRepository.GetAll());
		}

		public Task<OutageEntity> GetOutage(long gid)
		{
			return new Task<OutageEntity>(() => dbContext.OutageRepository.Get(gid));
		}

		public Task<bool> IsAlive()
		{
			return Task.Run(() => true);
		}

		public Task RemoveAllOutages()
		{
			return new Task(() =>
			{
				dbContext.OutageRepository.RemoveAll();
				dbContext.Complete();
			});
		}

		public Task RemoveOutage(OutageEntity outage)
		{
			return new Task(() =>
			{
				dbContext.OutageRepository.Remove(outage);
				dbContext.Complete();
			});
		}

		public Task UpdateOutage(OutageEntity outage)
		{
			return new Task(() =>
			{
				dbContext.OutageRepository.Update(outage);
				dbContext.Complete();
			});
		}
	}
}

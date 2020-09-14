using Common.OmsContracts.DataContracts.OutageDatabaseModel;
using Common.OmsContracts.ModelAccess;
using OMS.Common.Cloud.Logger;
using OutageDatabase.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.HistoryDBManagerImplementation.ModelAccess
{
    public class OutageModelAccess : IOutageAccessContract
	{
		private readonly string baseLogString;

		#region Private Properties
		private ICloudLogger logger;
		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}
		#endregion Private Properties

		public OutageModelAccess()
		{
			this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
			Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");
		}

		#region IOutageAccessContract
		public Task<OutageEntity> AddOutage(OutageEntity outage)
		{
			return Task.Run(() =>
			{
				//MODO: razmisliti o ConditonalValue<OutageEntity>
				OutageEntity outageEntityDb = null;

				using (var unitOfWork = new UnitOfWork())
				{
					try
					{
						outageEntityDb = unitOfWork.OutageRepository.Add(outage);
						unitOfWork.Complete();
					}
					catch (Exception e)
					{
						string message = $"{baseLogString} AddOutage => Exception: {e.Message}";
						Logger.LogError(message, e);
					}
				}

				return outageEntityDb;
			});
		}

		//public Task<IEnumerable<OutageEntity>> FindOutage(OutageExpression expression)
		//{
		//	return Task.Run(() =>
		//	{
		//		IEnumerable<OutageEntity> outageEntities = new List<OutageEntity>();

		//		using (var unitOfWork = new UnitOfWork())
		//		{
		//			try
		//			{
		//				outageEntities = unitOfWork.OutageRepository.Find(expression.Predicate);
		//			}
		//			catch (Exception e)
		//			{
		//				string message = $"{baseLogString} FindOutage => Exception: {e.Message}";
		//				Logger.LogError(message, e);
		//			}
		//		}

		//		return outageEntities;
		//	});
		//}

		public Task<IEnumerable<OutageEntity>> GetAllActiveOutages()
		{
			return Task.Run(() =>
			{
				IEnumerable<OutageEntity> outageEntities = new List<OutageEntity>();

				using (var unitOfWork = new UnitOfWork())
				{
					try
					{
						outageEntities = unitOfWork.OutageRepository.GetAllActive();
					}
					catch (Exception e)
					{
						string message = $"{baseLogString} GetAllActiveOutages => Exception: {e.Message}";
						Logger.LogError(message, e);
					}
				}

				return outageEntities;
			});
		}

		public Task<IEnumerable<OutageEntity>> GetAllArchivedOutages()
		{
			return Task.Run(() =>
			{
				IEnumerable<OutageEntity> outageEntities = new List<OutageEntity>();

				using (var unitOfWork = new UnitOfWork())
				{
					try
					{
						outageEntities = unitOfWork.OutageRepository.GetAllArchived();
					}
					catch (Exception e)
					{
						string message = $"{baseLogString} GetAllArchivedOutages => Exception: {e.Message}";
						Logger.LogError(message, e);
					}
				}

				return outageEntities;
			});
		}

		public Task<IEnumerable<OutageEntity>> GetAllOutages()
		{
			return Task.Run(() =>
			{
				IEnumerable<OutageEntity> outageEntities = new List<OutageEntity>();

				using (var unitOfWork = new UnitOfWork())
				{
					try
					{
						outageEntities = unitOfWork.OutageRepository.GetAll();
					}
					catch (Exception e)
					{
						string message = $"{baseLogString} GetAllOutages => Exception: {e.Message}";
						Logger.LogError(message, e);
					}
				}

				return outageEntities;
			});
		}

		public Task<OutageEntity> GetOutage(long gid)
		{
			return Task.Run(() =>
			{
				//MODO: razmisliti o ConditonalValue<OutageEntity>
				OutageEntity outageEntityDb = null;

				using (var unitOfWork = new UnitOfWork())
				{
					try
					{
						outageEntityDb = unitOfWork.OutageRepository.Get(gid);
					}
					catch (Exception e)
					{
						string message = $"{baseLogString} GetOutage => Exception: {e.Message}";
						Logger.LogError(message, e);
					}
				}

				return outageEntityDb;
			});
		}

		public Task RemoveAllOutages()
		{
			return Task.Run(() =>
			{
				using (var unitOfWork = new UnitOfWork())
				{
					try
					{
						unitOfWork.OutageRepository.RemoveAll();
						unitOfWork.Complete();
					}
					catch (Exception e)
					{
						string message = $"{baseLogString} RemoveAllOutages => Exception: {e.Message}";
						Logger.LogError(message, e);
					}
				}
			});
		}

		public Task RemoveOutage(OutageEntity outage)
		{
			return Task.Run(() =>
			{
				using (var unitOfWork = new UnitOfWork())
				{
					try
					{
						unitOfWork.OutageRepository.Remove(outage);
						unitOfWork.Complete();
					}
					catch (Exception e)
					{
						string message = $"{baseLogString} RemoveOutage => Exception: {e.Message}";
						Logger.LogError(message, e);
					}
				}
			});
		}

		public Task UpdateOutage(OutageEntity outage)
		{
			return Task.Run(() =>
			{
				using (var unitOfWork = new UnitOfWork())
				{
					try
					{
						unitOfWork.OutageRepository.Update(outage);
						unitOfWork.Complete();
					}
					catch (Exception e)
					{
						string message = $"{baseLogString} UpdateOutage => Exception: {e.Message}";
						Logger.LogError(message, e);
					}
				}
			});
		}

		public Task<bool> IsAlive()
		{
			return Task.Run(() => true);
		}
		#endregion IOutageAccessContract
	}
}

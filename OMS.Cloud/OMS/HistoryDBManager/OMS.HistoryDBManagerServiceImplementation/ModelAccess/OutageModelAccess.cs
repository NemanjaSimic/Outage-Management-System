using Common.OmsContracts.DataContracts.OutageDatabaseModel;
using Common.OmsContracts.ModelAccess;
using OMS.Common.Cloud.Logger;
using OutageDatabase.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

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
						List<Consumer> consumersFromDb = GetConsumersFromDb(outage.AffectedConsumers, unitOfWork);
						if (consumersFromDb.Count != outage.AffectedConsumers.Count)
						{
							Logger.LogError($"{baseLogString} AddOutage => Some of AffectedConsumers are not present in database.");
							return outageEntityDb;
						}

						List<Equipment> defaultIsolationPointsFromDb = GetEquipmentFromDb(outage.DefaultIsolationPoints, unitOfWork);
						if (defaultIsolationPointsFromDb.Count != outage.DefaultIsolationPoints.Count)
						{
							Logger.LogError($"{baseLogString} AddOutage => Some of DefaultIsolationPoints are not present in database.");
							return outageEntityDb;
						}

						List<Equipment> optimumIsolationPointsFromDb = GetEquipmentFromDb(outage.OptimumIsolationPoints, unitOfWork);
						if (optimumIsolationPointsFromDb.Count != outage.OptimumIsolationPoints.Count)
						{
							Logger.LogError($"{baseLogString} AddOutage => Some of OptimumIsolationPoints are not present in database.");
							return outageEntityDb;
						}

						outage.AffectedConsumers.Clear();
						outage.AffectedConsumers.AddRange(consumersFromDb);

						outage.DefaultIsolationPoints.Clear();
						outage.DefaultIsolationPoints.AddRange(defaultIsolationPointsFromDb);

						outage.OptimumIsolationPoints.Clear();
						outage.OptimumIsolationPoints.AddRange(optimumIsolationPointsFromDb);

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

		public Task<List<OutageEntity>> GetAllActiveOutages()
		{
			return Task.Run(() =>
			{
				List<OutageEntity> outageEntities = new List<OutageEntity>();

				using (var unitOfWork = new UnitOfWork())
				{
					try
					{
						outageEntities = new List<OutageEntity>(unitOfWork.OutageRepository.GetAllActive());
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

		public Task<List<OutageEntity>> GetAllArchivedOutages()
		{
			return Task.Run(() =>
			{
				List<OutageEntity> outageEntities = new List<OutageEntity>();

				using (var unitOfWork = new UnitOfWork())
				{
					try
					{
						outageEntities = new List<OutageEntity>(unitOfWork.OutageRepository.GetAllArchived());
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

		public Task<List<OutageEntity>> GetAllOutages()
		{
			return Task.Run(() =>
			{
				List<OutageEntity> outageEntities = new List<OutageEntity>();

				using (var unitOfWork = new UnitOfWork())
				{
					try
					{
						outageEntities = new List<OutageEntity>(unitOfWork.OutageRepository.GetAll());
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

		#region Private methods
		private List<Consumer> GetConsumersFromDb(List<Consumer> consumers, UnitOfWork unitOfWork)
		{
			List<Consumer> consumersFromDb = new List<Consumer>();
			foreach (var consumer in consumers)
			{
				Consumer consumerFromDb = null;
				if ((consumerFromDb = unitOfWork.ConsumerRepository.Get(consumer.ConsumerId)) == null)
				{
					break;
				}
				consumersFromDb.Add(consumerFromDb);
			}

			return consumersFromDb;
		}

		private List<Equipment> GetEquipmentFromDb(List<Equipment> equipments, UnitOfWork unitOfWork)
		{
			List<Equipment> equipmentsFromDb = new List<Equipment>();

			foreach (var eqipment in equipments)
			{
				Equipment equipmentFromDb = null;
				if ((equipmentFromDb = unitOfWork.EquipmentRepository.Get(eqipment.EquipmentId)) == null)
				{
					break;
				}
				equipmentsFromDb.Add(equipmentFromDb);
			}

			return equipmentsFromDb;

		}
		#endregion
	}
}

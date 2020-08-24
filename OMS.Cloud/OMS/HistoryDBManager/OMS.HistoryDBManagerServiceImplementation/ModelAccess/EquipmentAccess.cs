using Common.OmsContracts.DataContracts.OutageDatabaseModel;
using Common.OmsContracts.ModelAccess;
using OMS.Common.Cloud.Logger;
using OutageDatabase.Repository;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.HistoryDBManagerImplementation.ModelAccess
{
    public class EquipmentAccess : IEquipmentAccessContract
	{
		private readonly string baseLogString;

		#region Private Properties
		private ICloudLogger logger;
		private ICloudLogger Logger
		{
			get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
		}
		#endregion Private Properties

		public EquipmentAccess()
		{
			this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
			Logger.LogDebug($"{baseLogString} Ctor => Logger initialized");
		}

		#region IEquipmentAccessContract
		public Task<Equipment> AddEquipment(Equipment equipment)
		{
			return Task.Run(() =>
			{
				//MODO: razmisliti o ConditonalValue<Equipment>
				Equipment equipmentDb = null;

				using (var unitOfWork = new UnitOfWork())
				{
					try
					{
						equipmentDb = unitOfWork.EquipmentRepository.Add(equipment);
						unitOfWork.Complete();
					}
					catch (Exception e)
					{
						string message = $"{baseLogString} AddEquipment => Exception: {e.Message}";
						Logger.LogError(message, e);
					}
				}

				return equipmentDb;
			});
		}

		public Task<IEnumerable<Equipment>> FindEquipment(EquipmentExpression expression)
		{
			return Task.Run(() =>
			{
				IEnumerable<Equipment> equipment = new List<Equipment>();

				using (var unitOfWork = new UnitOfWork())
				{
					try
					{
						equipment = unitOfWork.EquipmentRepository.Find(expression.Predicate);
					}
					catch (Exception e)
					{
						string message = $"{baseLogString} FindEquipment => Exception: {e.Message}";
						Logger.LogError(message, e);
					}
				}

				return equipment;
			});
		}

		public Task<IEnumerable<Equipment>> GetAllEquipments()
		{
			return Task.Run(() =>
			{
				IEnumerable<Equipment> equipment = new List<Equipment>();

				using (var unitOfWork = new UnitOfWork())
				{
					try
					{
						equipment = unitOfWork.EquipmentRepository.GetAll();
					}
					catch (Exception e)
					{
						string message = $"{baseLogString} GetAllEquipments => Exception: {e.Message}";
						Logger.LogError(message, e);
					}
				}

				return equipment;
			});
		}

		public Task<Equipment> GetEquipment(long gid)
		{
			return Task.Run(() =>
			{
				//MODO: razmisliti o ConditonalValue<Equipment>
				Equipment equipment = null;

				using (var unitOfWork = new UnitOfWork())
				{
					try
					{
						equipment = unitOfWork.EquipmentRepository.Get(gid);
					}
					catch (Exception e)
					{
						string message = $"{baseLogString} GetEquipment => Exception: {e.Message}";
						Logger.LogError(message, e);
					}
				}

				return equipment;
			});
		}

		public Task RemoveAllEquipments()
		{
			return Task.Run(() =>
			{
				using (var unitOfWork = new UnitOfWork())
				{
					try
					{
						unitOfWork.EquipmentRepository.RemoveAll();
						unitOfWork.Complete();
					}
					catch (Exception e)
					{
						string message = $"{baseLogString} RemoveAllEquipments => Exception: {e.Message}";
						Logger.LogError(message, e);
					}
				}
			});
		}

		public Task RemoveEquipment(Equipment equipment)
		{
			return Task.Run(() =>
			{
				using (var unitOfWork = new UnitOfWork())
				{
					try
					{
						unitOfWork.EquipmentRepository.Remove(equipment);
						unitOfWork.Complete();
					}
					catch (Exception e)
					{
						string message = $"{baseLogString} RemoveEquipment => Exception: {e.Message}";
						Logger.LogError(message, e);
					}
				}
			});
		}

		public Task UpdateEquipment(Equipment equipment)
		{
			return Task.Run(() =>
			{
				using (var unitOfWork = new UnitOfWork())
				{
					try
					{
						unitOfWork.EquipmentRepository.Update(equipment);
						unitOfWork.Complete();
					}
					catch (Exception e)
					{
						string message = $"{baseLogString} UpdateEquipment => Exception: {e.Message}";
						Logger.LogError(message, e);
					}
				}
			});
		}

		public Task<bool> IsAlive()
		{
			return Task.Run(() => true);
		}
		#endregion IEquipmentAccessContract
	}
}

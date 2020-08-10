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
	public class EquipmentAccess : IEquipmentAccessContract
	{
		private UnitOfWork dbContext;

		public EquipmentAccess()
		{
			this.dbContext = new UnitOfWork();
			
		}
		public Task<Equipment> AddEquipment(Equipment equipment)
		{
			return new Task<Equipment>(() =>
			{
				Equipment equipmentDb = dbContext.EquipmentRepository.Add(equipment);
				dbContext.Complete();
				return equipmentDb;
			});
		}

		public Task<IEnumerable<Equipment>> FindEquipment(Expression<Func<Equipment, bool>> predicate)
		{
			return new Task<IEnumerable<Equipment>>(() => dbContext.EquipmentRepository.Find(predicate));
		}

		public Task<IEnumerable<Equipment>> GetAllEquipments()
		{
			return new Task<IEnumerable<Equipment>>(() => dbContext.EquipmentRepository.GetAll());
		}

		public Task<Equipment> GetEquipment(long gid)
		{
			return new Task<Equipment>(() => dbContext.EquipmentRepository.Get(gid));
		}

		public Task RemoveAllEquipments()
		{
			return new Task(() => 
			{
				dbContext.EquipmentRepository.RemoveAll();
				dbContext.Complete();
			});
		}

		public Task RemoveEquipment(Equipment equipment)
		{
			return new Task(() =>
			{
				dbContext.EquipmentRepository.Remove(equipment);
				dbContext.Complete();
			});
		}

		public Task UpdateEquipment(Equipment equipment)
		{
			return new Task(() =>
			{
				dbContext.EquipmentRepository.Update(equipment);
				dbContext.Complete();
			});
		}
	}
}

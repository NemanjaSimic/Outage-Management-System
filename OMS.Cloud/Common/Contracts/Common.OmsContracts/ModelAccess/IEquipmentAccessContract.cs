using Common.CloudContracts;
using Common.OMS.OutageDatabaseModel;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common.OmsContracts.ModelAccess
{
	[ServiceContract]
	public interface IEquipmentAccessContract : IService, IHealthChecker
	{
		[OperationContract]
		Task<IEnumerable<Equipment>> GetAllEquipments();
		[OperationContract]
		Task<Equipment> GetEquipment(long gid);
		[OperationContract]
		Task<Equipment> AddEquipment(Equipment equipment);
		[OperationContract]
		Task UpdateEquipment(Equipment equipment);
		[OperationContract]
		Task RemoveEquipment(Equipment equipment);
		[OperationContract]
		Task RemoveAllEquipments();
		[OperationContract]
		Task<IEnumerable<Equipment>> FindEquipment(Expression<Func<Equipment, bool>> predicate);
	}
}

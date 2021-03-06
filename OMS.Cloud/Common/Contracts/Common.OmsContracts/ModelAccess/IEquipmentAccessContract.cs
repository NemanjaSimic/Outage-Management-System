﻿using Common.CloudContracts;
using Common.OmsContracts.DataContracts.OutageDatabaseModel;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.OmsContracts.ModelAccess
{
    [ServiceContract]
	public interface IEquipmentAccessContract : IService, IHealthChecker
	{
		[OperationContract]
		Task<List<Equipment>> GetAllEquipments();

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
    }
}

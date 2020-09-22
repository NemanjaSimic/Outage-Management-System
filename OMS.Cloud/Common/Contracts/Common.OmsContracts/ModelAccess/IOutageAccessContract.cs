using Common.CloudContracts;
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
	public interface IOutageAccessContract : IService, IHealthChecker
	{
		[OperationContract]
		Task<List<OutageEntity>> GetAllActiveOutages();

		[OperationContract]
		Task<List<OutageEntity>> GetAllArchivedOutages();

		[OperationContract]
		Task<List<OutageEntity>> GetAllOutages();

		[OperationContract]
		Task<OutageEntity> AddOutage(OutageEntity outage);

		[OperationContract]
		Task<OutageEntity> GetOutage(long gid);

		[OperationContract]
		Task RemoveOutage(OutageEntity outage);

		[OperationContract]
		Task RemoveAllOutages();

		[OperationContract]
		Task UpdateOutage(OutageEntity outage);
    }
}

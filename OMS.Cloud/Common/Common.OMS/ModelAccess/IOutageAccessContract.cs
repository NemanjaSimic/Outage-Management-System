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
	public interface IOutageAccessContract : IService
	{
		[OperationContract]
		Task<IEnumerable<OutageEntity>> GetAllActiveOutages();
		[OperationContract]
		Task<IEnumerable<OutageEntity>> GetAllArchivedOutages();
		[OperationContract]
		Task<IEnumerable<OutageEntity>> GetAllOutages();
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
		[OperationContract]
		Task<IEnumerable<OutageEntity>> FindOutage(Expression<Func<OutageEntity, bool>> predicate);
	}
}

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
        Task<IEnumerable<OutageEntity>> FindOutage(OutageExpression expression);
    }

    [DataContract]
    public class OutageExpression
    {
		[DataMember]
        public Expression<Func<OutageEntity, bool>> Predicate { get; set; }
    }
}

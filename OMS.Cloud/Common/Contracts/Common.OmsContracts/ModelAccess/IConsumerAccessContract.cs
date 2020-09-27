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
	public interface IConsumerAccessContract : IService, IHealthChecker
	{
		[OperationContract]
		Task<List<Consumer>> GetAllConsumers();

		[OperationContract]
		Task<Consumer> GetConsumer(long gid);

		[OperationContract]
		Task<Consumer> AddConsumer(Consumer consumer);

		[OperationContract]
		Task UpdateConsumer(Consumer consumer);

		[OperationContract]
		Task RemoveConsumer(Consumer consumer);

		[OperationContract]
		Task RemoveAllConsumers();
    }
}

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
	public interface IConsumerAccessContract : IService
	{
		[OperationContract]
		Task<IEnumerable<Consumer>> GetAllConsumers();
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
		[OperationContract]
		Task<IEnumerable<Consumer>> FindConsumer(Expression<Func<Consumer, bool>> predicate);
	}
}

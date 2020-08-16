using Common.CloudContracts;
using Common.OMS.OutageDatabaseModel;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common.OmsContracts.ModelAccess
{
	[ServiceContract]
	public interface IConsumerAccessContract : IService, IHealthChecker
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

		//TODO: solve serialization of Expression<Func<Consumer, bool>>
		//[OperationContract]
		//Task<IEnumerable<Consumer>> FindConsumer(Expression<Func<Consumer, bool>> predicate);
	}

	//TODO: probati
	//[DataContract]
	//public class Foo
	//{
	//	[DataMember]
	//	public Expression<Func<Consumer, bool>> FooProp { get; set; }
	//}
}

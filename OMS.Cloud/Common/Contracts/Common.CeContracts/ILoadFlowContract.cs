using Common.CloudContracts;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.CeContracts.LoadFlow
{
	[ServiceContract]
	public interface ILoadFlowContract : IService, IHealthChecker
	{
		[OperationContract]
		[ServiceKnownType(typeof(EnergyConsumer))]
		[ServiceKnownType(typeof(Feeder))]
		[ServiceKnownType(typeof(Field))]
		[ServiceKnownType(typeof(Recloser))]
		[ServiceKnownType(typeof(SynchronousMachine))]
		[ServiceKnownType(typeof(TopologyElement))]
		Task<TopologyModel> UpdateLoadFlow(TopologyModel topology);
	}
}

using Common.CloudContracts;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.CeContracts
{
	[ServiceContract]
	public interface ITopologyBuilderContract : IService, IHealthChecker
	{
		[OperationContract]
		[ServiceKnownType(typeof(TopologyModel))]
		[ServiceKnownType(typeof(EnergyConsumer))]
		[ServiceKnownType(typeof(Feeder))]
		[ServiceKnownType(typeof(Field))]
		[ServiceKnownType(typeof(Recloser))]
		[ServiceKnownType(typeof(SynchronousMachine))]
		[ServiceKnownType(typeof(TopologyElement))]
		Task<ITopology> CreateGraphTopology(long firstElementGid, string whoIsCalling);
	}
}

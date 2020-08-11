using Common.CE.Interfaces;
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
		Task<ITopology> CreateGraphTopology(long firstElementGid);
	}
}

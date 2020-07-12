using CECommon.Interfaces;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.CeContracts
{
	[ServiceContract]
	public interface ITopologyBuilderService : IService
	{
		[OperationContract]
		Task<ITopology> CreateGraphTopology(long firstElementGid);
	}
}

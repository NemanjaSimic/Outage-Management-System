using CECommon.Interfaces;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.CeContracts.LoadFlow
{
	[ServiceContract]
	public interface ILoadFlowContract : IService
	{
		[OperationContract]
		Task<ITopology> UpdateLoadFlow(ITopology topology);
	}
}

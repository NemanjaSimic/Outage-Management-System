using Common.CE.Interfaces;
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
		Task<ITopology> UpdateLoadFlow(ITopology topology);
	}
}

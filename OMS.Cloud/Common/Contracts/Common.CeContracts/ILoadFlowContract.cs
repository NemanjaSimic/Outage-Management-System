using CECommon.Interfaces;
using Microsoft.ServiceFabric.Services.Remoting;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.CeContracts.LoadFlow
{
	[ServiceContract]
	public interface ILoadFlowContract : IService
	{
		[OperationContract]
		Task UpdateLoadFlow(List<ITopology> topologies);
	}
}

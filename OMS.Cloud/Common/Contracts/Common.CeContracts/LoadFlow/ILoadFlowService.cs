using CECommon.Interfaces;
using Microsoft.ServiceFabric.Services.Remoting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.CeContracts.LoadFlow
{
	public interface ILoadFlowService : IService
	{
		Task UpdateLoadFlow(List<ITopology> topologies);
	}
}

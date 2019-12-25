using CECommon.Model;
using CECommon.ServiceContracts;
using System;

namespace CalculationEngineService
{
	public class TopologyService : ITopologyServiceContract
	{
        public TopologyModel GetTopology()
        {
			try
			{
				return Topology.Topology.Instance.TopologyModel;
			}
			catch (Exception)
			{

				throw;
			}
        }
    }
}

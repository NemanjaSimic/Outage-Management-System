using CECommon.Model;
using CECommon.Model.UI;
using CECommon.ServiceContracts;
using System;

namespace CalculationEngineService
{
	public class TopologyService : ITopologyServiceContract
	{
        public UIModel GetTopology()
        {
			try
			{
				return Topology.Topology.Instance.TopologyModel.UIModel;
			}
			catch (Exception)
			{

				throw;
			}
        }

		//private UIModel PrepareTopology(TopologyElement firstElement)
		//{
		//	UIModel uIModel = new UIModel();
			
		//	return uIModel;
		//}
    }
}

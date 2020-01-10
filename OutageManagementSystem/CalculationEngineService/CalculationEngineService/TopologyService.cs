using CECommon.Model;
using CECommon.Model.UI;
using CECommon.ServiceContracts;
using Outage.Common;
using System;

namespace CalculationEngineService
{
	public class TopologyService : ITopologyServiceContract
	{
		private ILogger logger = LoggerWrapper.Instance;
		public UIModel GetTopology()
        {
			try
			{
				return Topology.Topology.Instance.TopologyModel.UIModel;
			}
			catch (Exception ex)
			{
				string message = $"Topology service failed to return a topology model. Exception message: " + ex.Message;
				logger.LogError(message);
				throw ex;
			}
        }

		//private UIModel PrepareTopology(TopologyElement firstElement)
		//{
		//	UIModel uIModel = new UIModel();
			
		//	return uIModel;
		//}
    }
}

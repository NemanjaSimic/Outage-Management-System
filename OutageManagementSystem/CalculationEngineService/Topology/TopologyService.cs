using Outage.Common;
using Outage.Common.ServiceContracts;
using Outage.Common.UI;
using System;

namespace Topology
{
	public class TopologyService : ITopologyServiceContract
	{
		private ILogger logger = LoggerWrapper.Instance;
		public UIModel GetTopology()
		{
			try
			{
				return TopologyManager.Instance.TopologyModel.UIModel;
			}
			catch (Exception ex)
			{
				string message = $"Topology service failed to return a topology model. Exception message: " + ex.Message;
				logger.LogError(message);
				throw ex;
			}
		}
	}
}

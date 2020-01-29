using CECommon.Interfaces;
using Outage.Common;
using Outage.Common.ServiceContracts;
using Outage.Common.UI;
using System;
using System.Linq;

namespace Topology
{
	public class TopologyService : ITopologyServiceContract
	{
		private ILogger logger = LoggerWrapper.Instance;
		private IWebTopologyBuilder webTopologyBuilder = new WebTopologyBuilder();
		public UIModel GetTopology()
		{
			try
			{
				UIModel uIModel = new UIModel();
				if (TopologyManager.Instance.TopologyModel.Count > 0)
				{
					uIModel = webTopologyBuilder.CreateTopologyForWeb(TopologyManager.Instance.TopologyModel.First());
				}
				return uIModel; 
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

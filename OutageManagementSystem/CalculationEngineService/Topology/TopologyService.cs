using CECommon.Interfaces;
using CECommon.Providers;
using Outage.Common;
using Outage.Common.ServiceContracts;
using Outage.Common.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Topology
{
	public class TopologyService : ITopologyServiceContract
	{
		private ILogger logger = LoggerWrapper.Instance;
		public UIModel GetTopology()
		{
			try
			{
				List<UIModel> uIModels = Provider.Instance.WebTopologyModelProvider.GetUIModels();
				if (uIModels.Count > 0)
				{
					return uIModels.First();
				}
				else
				{
					//privremeno jer se salje jedna topologija
					return new UIModel();
				}
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

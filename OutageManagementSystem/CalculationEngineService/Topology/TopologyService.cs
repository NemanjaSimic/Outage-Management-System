using CECommon.Providers;
using Outage.Common;
using Outage.Common.OutageService.Interface;
using Outage.Common.OutageService.Model;
using Outage.Common.ServiceContracts;
using Outage.Common.ServiceContracts.CalculationEngine;
using Outage.Common.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Topology
{
	public class TopologyService : ITopologyServiceContract, ITopologyOMSService
	{
		private readonly ILogger logger = LoggerWrapper.Instance;

		public IOutageTopologyModel GetOMSModel()
		{
			try
			{
				List<IOutageTopologyModel> omsModels = Provider.Instance.TopologyConverterProvider.GetOMSModel();
				if (omsModels.Count > 0)
				{
					//privremeno jer se salje jedna topologija
					return omsModels.First();
				}
				else
				{
					return new OutageTopologyModel();
				}
			}
			catch (Exception ex)
			{
				logger.LogError($"Topology service failed to return a oms model. Exception message: { ex.Message}");
				throw;
			}
		}

		public UIModel GetTopology()
		{
			try
			{
				List<UIModel> uIModels = Provider.Instance.TopologyConverterProvider.GetUIModels();
				if (uIModels.Count > 0)
				{
					//privremeno jer se salje jedna topologija
					return uIModels.First();
				}
				else
				{ 
					return new UIModel();
				}
			}
			catch (Exception ex)
			{
				logger.LogError($"Topology service failed to return a topology model. Exception message: {ex.Message}");
				throw;
			}
		}
	}
}

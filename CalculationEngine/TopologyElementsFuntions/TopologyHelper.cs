using CECommon;
using CECommon.TopologyConfiguration;
using Outage.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopologyElementsFuntions
{
	public class TopologyHelper
	{
		private Dictionary<ElementStatus, List<DMSType>> elementsStatus;
		private Dictionary<TopologyType, List<DMSType>> topologyTypes;

		public TopologyHelper()
		{
			ConfigurationParse cp = new ConfigurationParse();
			elementsStatus = cp.GetAllElementStatus();
			topologyTypes = cp.GetAllTopologyTypes();
		}

		public ElementStatus GetElementStatus(DMSType type)
		{
			ElementStatus retVal;
			foreach (var item in elementsStatus)
			{
				if (item.Value.Contains(type))
				{
					retVal = item.Key;
					return retVal;
				}
			}
			return ElementStatus.Regular;
		}

		public TopologyType GetTopologyType(DMSType type)
		{
			TopologyType retVal;
			foreach (var item in topologyTypes)
			{
				
				if (item.Value.Contains(type))
				{
					retVal = item.Key;
					return retVal;
				}
			}
			return TopologyType.None;
		}
	}
}

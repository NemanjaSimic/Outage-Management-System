using CECommon;
using Outage.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopologyElementsFuntions
{
	public class TopologyElementFactory
	{
		public TopologyElement CreateElement(ModelCode type, long gid)
		{
			TopologyElement retVal;
			TopologyHelper topologyHelper = new TopologyHelper();
			DMSType dMSType = ModelCodeHelper.GetTypeFromModelCode(type);
			TopologyType dmsTopologyType = topologyHelper.GetTopologyType(dMSType);

			if (dmsTopologyType == TopologyType.Edge)
				retVal = new Edge(gid);
			else if (dmsTopologyType == TopologyType.Node)
				retVal = new RegularNode(gid);
			else
				retVal = null;

			return retVal;
		}
	}
}

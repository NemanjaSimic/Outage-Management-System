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
		private static long edgeCounter = 0;
		public static TopologyElement CreateTopologyElement(long gid)
		{
			TopologyElement retVal;
			ModelResourcesDesc modelResourcesDesc = new ModelResourcesDesc();
			TopologyHelper topologyHelper = new TopologyHelper();

			TopologyType dmsTopologyType = topologyHelper.GetElementTopologyType(gid);

			if (dmsTopologyType == TopologyType.Edge)
				retVal = new Edge(gid);
			else if (dmsTopologyType == TopologyType.Node)
				retVal = new RegularNode(gid, topologyHelper.GetElementTopologyStatus(gid));
			else
				retVal = null;
			// ovde treba baciti exception umesto vratiti null
			return retVal;
		}

		public static Edge CreateOrdinaryEdge(long firstEndGid, long secondEndGid)
		{
			return new Edge(edgeCounter++) {FirstEnd = firstEndGid, SecondEnd = secondEndGid };
		}
	}
}

using CECommon;
using Outage.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TopologyElementsFuntions
{
	public class TopologyElementFactory
	{
		private static long edgeCounter = 0;
		public TopologyElement CreateTopologyElement(long gid)
		{
			TopologyElement retVal;
			TopologyType dmsTopologyType = TopologyHelper.Instance.GetElementTopologyType(gid);
	
			if (dmsTopologyType == TopologyType.Edge)
				retVal = new Edge(gid);
			else if (dmsTopologyType == TopologyType.Node)
				retVal = new Node(gid);
			else
			{
				string message = $"Element with GID: {gid.ToString("X")} is neither Edge nor Node. Please check configuration files.";
				throw new Exception(message);
			}
			return retVal;
		}
		public Edge CreateOrdinaryEdge(TopologyElement firstEndGid, TopologyElement secondEndGid)
		{
			return new Edge(edgeCounter++) {FirstEnd = firstEndGid, SecondEnd = new List<TopologyElement>() { secondEndGid }};
		}
	}
}

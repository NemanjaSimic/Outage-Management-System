using CECommon;
using Outage.Common;
using System;
using System.Collections.Generic;

namespace TopologyElementsFuntions
{
	public class TopologyElementFactory
	{
		private static long edgeCounter = 0;
		private ILogger logger = LoggerWrapper.Instance;
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
				logger.LogError(message);
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

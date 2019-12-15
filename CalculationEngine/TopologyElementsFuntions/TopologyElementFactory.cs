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
		private TopologyHelper topologyHelper = new TopologyHelper();
		public TopologyElement CreateTopologyElement(long gid)
		{
			TopologyElement retVal;
			
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Restart();
			TopologyType dmsTopologyType = topologyHelper.GetElementTopologyType(gid);
			stopwatch.Stop();
			//Console.WriteLine("Getting element DMSType for " + stopwatch.Elapsed.ToString());

			stopwatch.Restart();
			if (dmsTopologyType == TopologyType.Edge)
				retVal = new Edge(gid);
			else if (dmsTopologyType == TopologyType.Node)
				retVal = new RegularNode(gid, topologyHelper.GetElementTopologyStatus(gid));
			else
			{
				string message = $"Element with GID: {gid.ToString("X")} is neither Edge nor Node. Please check configuration files.";
				Exception ex = new Exception(message);
				throw ex;
			}
			stopwatch.Stop();
			//Console.WriteLine("Created new element for " + stopwatch.Elapsed.ToString());
			return retVal;
		}
		public Edge CreateOrdinaryEdge(TopologyElement firstEndGid, TopologyElement secondEndGid)
		{
			return new Edge(edgeCounter++) {FirstEnd = firstEndGid, SecondEnd = new List<TopologyElement>() { secondEndGid }};
		}
	}
}

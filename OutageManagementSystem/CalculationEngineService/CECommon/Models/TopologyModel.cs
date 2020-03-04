using CECommon.Interfaces;
using System.Collections.Generic;
using Logger = Outage.Common.LoggerWrapper;

namespace CECommon.Model
{
	public class TopologyModel : ITopology
    {
		public long FirstNode { get; set; }
		public Dictionary<long, ITopologyElement> TopologyElements { get; set; }
		public TopologyModel()
		{
			TopologyElements = new Dictionary<long, ITopologyElement>();
		}
		public void AddElement(ITopologyElement newElement)
		{
			if (!TopologyElements.ContainsKey(newElement.Id))
			{
				TopologyElements.Add(newElement.Id, newElement);
			}
			else
			{
				Logger.Instance.LogWarn($"Topology element with GID 0x{newElement.Id.ToString("X16")} is already added.");
			}
		}
		public bool GetElementByGid(long gid, out ITopologyElement topologyElement)
		{
			bool success = false;
			if (TopologyElements.ContainsKey(gid))
			{
				topologyElement = TopologyElements[gid];
				success = true;
			}
			else
			{
				topologyElement = null;
			}
			return success;
		}

	}
}

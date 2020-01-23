using CECommon.Interfaces;
using Outage.Common.UI;
using System.Collections.Generic;
using Logger = Outage.Common.LoggerWrapper;

namespace CECommon.Model
{

	public class TopologyModel : ITopology
    {
		private long firstNode;
		private Dictionary<long, ITopologyElement> topologyElements;

		public long FirstNode
		{
			get { return firstNode; }
			set 
			{ 
				firstNode = value;
				//UIModel.FirstNode = firstNode.Id;
			}
		}
		public UIModel UIModel { get; set; }
		public Dictionary<long, ITopologyElement> TopologyElements { get => topologyElements; set => topologyElements = value; }
		public TopologyModel()
		{
			TopologyElements = new Dictionary<long, ITopologyElement>();
			UIModel = new UIModel();
		}
		public void AddRelation(long source, long destination)
		{
			UIModel.AddRelation(source, destination);
		}
		public void AddElement(ITopologyElement newElement)
		{
			if (!TopologyElements.ContainsKey(newElement.Id))
			{
				TopologyElements.Add(newElement.Id, newElement);
			}
			else
			{
				Logger.Instance.LogWarn($"Topology element with GID {newElement.Id} is already added.");
			}
		}
		public bool GetElementByGid(long gid, out ITopologyElement topologyElement)
		{
			bool success = false;
			if (topologyElements.ContainsKey(gid))
			{
				topologyElement = topologyElements[gid];
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

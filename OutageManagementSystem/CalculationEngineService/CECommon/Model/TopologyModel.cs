using Outage.Common.UI;
using System.Collections.Generic;

namespace CECommon.Model
{

	public class TopologyModel
    {
		private TopologyElement firstNode;
		private Dictionary<long, TopologyElement> topologyElements;

		public TopologyElement FirstNode
		{
			get { return firstNode; }
			set 
			{ 
				firstNode = value;
				UIModel.FirstNode = firstNode.Id;
			}
		}
		public UIModel UIModel { get; set; }
		public Dictionary<long, TopologyElement> TopologyElements { get => topologyElements; set => topologyElements = value; }

		public TopologyModel()
		{
			TopologyElements = new Dictionary<long, TopologyElement>();
			UIModel = new UIModel();
		}
		public void AddRelation(long source, long destination)
		{
			UIModel.AddRelation(source, destination);
		}
		public void AddElement(TopologyElement newElement)
		{
			if (!TopologyElements.ContainsKey(newElement.Id))
			{
				TopologyElements.Add(newElement.Id, newElement);
			}
			UIModel.AddNode(new UINode(newElement.Id, newElement.DmsType));
		}
		
	}
}

using CECommon.Model.UI;
using System.Runtime.Serialization;

namespace CECommon.Model
{

	public class TopologyModel
    {
		private TopologyElement firstNode;

		[DataMember]
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
		public TopologyModel()
		{
			UIModel = new UIModel();
		}
		public void AddRelation(long source, long destination)
		{
			UIModel.AddRelation(source, destination);
		}
		public void AddUINode(UINode newUiNode)
		{
			UIModel.AddNode(newUiNode);
		}
		
	}
}

using System.Collections.Generic;

namespace Common.PubSubContracts.DataContracts.CE.Interfaces
{
	public interface IUIModel
	{
		long FirstNode { get; set; }
		Dictionary<long, IUINode> Nodes { get; set; }
		Dictionary<long, HashSet<long>> Relations { get; set; }

		void AddNode(IUINode newNode);
		void AddRelation(long source, long destination);
		HashSet<long> GetRelatedElements(long sourceGid);
	}
}
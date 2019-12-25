using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CECommon.Model
{
	[DataContract]
	public class TopologyModel
    {
		private Dictionary<long,TopologyElement> nodes;
		private Dictionary<long, HashSet<long>> relations;
		private TopologyElement firstNode;
		[DataMember]
		public TopologyElement FirstNode
		{
			get { return firstNode; }
			set { firstNode = value; }
		}
		[DataMember]
		public Dictionary<long, TopologyElement> Nodes
		{
			get { return nodes; }
			private set { nodes = value; }
		}
		[DataMember]
		public Dictionary<long, HashSet<long>> Relations
		{
			get { return relations;  }
			private set { relations = value; }
		}

		public TopologyModel()
		{
			Nodes = new Dictionary<long, TopologyElement>();
			Relations = new Dictionary<long, HashSet<long>>();
		}

		public void AddRelation(long source, long destination)
		{
			if (Relations.ContainsKey(source))
			{
				try
				{
					Relations[source].Add(destination);
				}
				catch (Exception)
				{
					throw new Exception($"Relaton {source} - {destination} already exists.");
				}
			}
			else
			{
				Relations.Add(source, new HashSet<long>() { destination });
			}
		}
		public void AddNode(TopologyElement newNode)
		{
			if (!Nodes.ContainsKey(newNode.Id))
			{
				Nodes.Add(newNode.Id, newNode);
			}
		}
		public HashSet<long> GetRelatedElements(long sourceGid)
		{
			if (Relations.ContainsKey(sourceGid))
			{
				return Relations[sourceGid];
			}

			return null;
		}
		
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon.Model
{
    public class Topology
    {
		private Dictionary<long,TopologyElement> nodes;
		private Dictionary<long, HashSet<long>> relations;
		private TopologyElement firstNode;

		public TopologyElement FirstNode
		{
			get { return firstNode; }
			set { firstNode = value; }
		}

		public Dictionary<long, TopologyElement> Nodes
		{
			get { return nodes; }
			private set { nodes = value; }
		}

		public Dictionary<long, HashSet<long>> Relations
		{
			get { return relations;  }
			private set { relations = value; }
		}

		public Topology()
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

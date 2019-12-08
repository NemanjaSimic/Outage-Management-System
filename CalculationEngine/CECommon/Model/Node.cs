using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon
{
    public abstract class Node : TopologyElement
    {
		private long? parent;
		private List<Node> children;
		private List<long> secondEnd;

		public long? Parent { get => parent; set => parent = value; }
		public List<Node> Children { get => children; set => children = value; }
		public List<long> SecondEnd { get => secondEnd; set => secondEnd = value; }

		public Node(long gid) : base (gid)
		{
			Children = new List<Node>();
			SecondEnd = new List<long>();
		}


	}
}

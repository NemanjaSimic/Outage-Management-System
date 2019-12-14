using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon
{
    public abstract class Node : TopologyElement
    {
		private TopologyElement parent;
		private List<long> children;
	
		public TopologyElement Parent { get => parent; set => parent = value; }
		public List<long> Children { get => children; set => children = value; }
		public Node(long gid) : base (gid)
		{
			Children = new List<long>();
		}


	}
}

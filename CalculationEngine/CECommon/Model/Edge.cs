using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon
{
	public class Edge : TopologyElement
	{
		private long secondEnd;

		public long SecondEnd { get => secondEnd; set => secondEnd = value; }

		public Edge(long gid) : base (gid)
		{

		}

	}
}

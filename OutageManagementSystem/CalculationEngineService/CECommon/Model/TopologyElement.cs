using Outage.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon
{
	public abstract class TopologyElement
	{
		private long id;
		private TopologyElement firstEnd;
		private List<TopologyElement> secondEnd;


		public long Id { get => id; set => id = value; }
		public TopologyElement FirstEnd { get => firstEnd; set => firstEnd = value; }
		public List<TopologyElement> SecondEnd { get => secondEnd; set => secondEnd = value; }

		public TopologyElement(long gid)
		{
			Id = gid;
			SecondEnd = new List<TopologyElement>();

		}
	}
}

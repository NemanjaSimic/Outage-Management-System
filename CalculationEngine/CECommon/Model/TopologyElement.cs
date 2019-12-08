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
		private long? firstEnd;

		public long Id { get => id; set => id = value; }
		public long? FirstEnd { get => firstEnd; set => firstEnd = value; }
		
		public TopologyElement(long gid)
		{
			Id = gid;

		}
	}
}

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
		public long Id { get => id; set => id = value; }

		public TopologyElement(long gid)
		{
			this.id = gid;
		}
	}
}

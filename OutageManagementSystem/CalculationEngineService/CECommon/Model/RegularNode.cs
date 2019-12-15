using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon
{
	public class RegularNode : Node
	{
		private TopologyStatus topologyStatus;
		public TopologyStatus TopologyStatus { get => topologyStatus; set => topologyStatus = value; }
		public RegularNode(long gid, TopologyStatus topologyStatus) : base(gid)
		{
			TopologyStatus = topologyStatus;
		}

	}
}

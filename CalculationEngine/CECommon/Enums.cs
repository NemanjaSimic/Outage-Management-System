using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon
{
	public enum TopologyType
	{
		Node = 1,
		Edge,
		None
	}

	public enum ElementStatus
	{
		Ignorable = 1,
		Field,
		Regular
	}
}

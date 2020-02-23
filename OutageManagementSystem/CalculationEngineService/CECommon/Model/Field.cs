using CECommon.Interfaces;
using System.Collections.Generic;

namespace CECommon
{
	public class Field : TopologyElement
	{
		private static long fieldNumber = 5000;
		private List<ITopologyElement> members;
		public List<ITopologyElement> Members { get => members; set => members = value; }
		public Field(ITopologyElement firstElement) : base(fieldNumber++)
		{
			Members = new List<ITopologyElement>() { firstElement};
			
		}
	}
}

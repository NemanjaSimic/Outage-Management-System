using CECommon.Interfaces;
using System.Collections.Generic;

namespace CECommon
{
	public class Field : TopologyElement
	{
		private static long fieldNumber = 5000;
		public List<ITopologyElement> Members { get; set; }
		public Field(ITopologyElement firstElement) : base(++fieldNumber)
		{
			Members = new List<ITopologyElement>() { firstElement};
			Mrid = $"F_{fieldNumber}";
			Name = $"F_{fieldNumber}";
		}
	}
}

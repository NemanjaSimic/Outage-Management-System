using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon
{
	public class Field : Edge
	{
		private static long fieldNumber = 5000;
		private List<TopologyElement> members;
		public List<TopologyElement> Members { get => members; set => members = value; }
		public Field(TopologyElement firstElement) : base(fieldNumber++)
		{
			Members = new List<TopologyElement>() { firstElement};
			
		}
	}
}

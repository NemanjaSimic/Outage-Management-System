using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon
{
	public class Field : Node
	{
		private static long fieldNumber = 5000;
		private List<long> members;
		public List<long> Members { get => members; set => members = value; }
		public Field(TopologyElement firstElement) : base(fieldNumber++)
		{
			Members = new List<long>() { firstElement.Id};
			
		}
	}
}

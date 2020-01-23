using System.Collections.Generic;

namespace CECommon
{
	public class Field : Edge
	{
		private static long fieldNumber = 5000;
		private List<long> members;
		public List<long> Members { get => members; set => members = value; }
		public Field(long firstElement) : base(fieldNumber++)
		{
			Members = new List<long>() { firstElement};
			
		}
	}
}

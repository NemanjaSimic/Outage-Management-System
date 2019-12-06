using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon
{
	public class Field : Node
	{
		private List<RegularNode> members;
		public List<RegularNode> Members { get => members; set => members = value; }
		public Field(long gid) : base(gid)
		{
		}
	}
}

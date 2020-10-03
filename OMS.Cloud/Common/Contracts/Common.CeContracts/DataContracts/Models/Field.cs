using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Common.CeContracts
{
	[DataContract]
	public class Field : TopologyElement
	{
		private static long fieldNumber = 5000;
		[DataMember]
		public List<TopologyElement> Members { get; set; }
		public Field(TopologyElement firstElement) : base(++fieldNumber)
		{
			Members = new List<TopologyElement>() { firstElement};
			Mrid = $"F_{fieldNumber}";
			Name = $"F_{fieldNumber}";
		}
	}
}

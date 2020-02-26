using CECommon.Interfaces;
using CECommon.Model;
using System;
using System.Collections.Generic;

namespace CECommon
{
	public class TopologyElement : ITopologyElement
	{
		#region Properties
		public long Id { get; set; }
		public string Description { get; set; }
		public string Mrid { get; set; }
		public string Name { get; set; }
		public float NominalVoltage { get; set; }
		public ITopologyElement FirstEnd { get; set; }
		public List<ITopologyElement> SecondEnd { get; set; }
		public string DmsType { get; set; }
		public List<long> Measurements { get; set; }
		public bool IsRemote { get; set; }
		public bool IsActive { get; set; }
		public bool NoReclosing { get; set; }

		#endregion
		public TopologyElement(long gid)
		{
			Id = gid;
			SecondEnd = new List<ITopologyElement>();
			Measurements = new List<long>();
		}
	}
}

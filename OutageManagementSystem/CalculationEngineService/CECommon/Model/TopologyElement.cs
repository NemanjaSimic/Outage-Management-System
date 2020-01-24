using CECommon.Interfaces;
using CECommon.Model;
using System.Collections.Generic;

namespace CECommon
{
	public abstract class TopologyElement : ITopologyElement
	{
        #region Fields
        private long id;
		private long firstEnd;
		private List<long> secondEnd;
		private string dmsType;
		private IMeasurement measurement;
		private string descritption;
		private string mrid;
		private string name;
		private bool isRemote;
		private bool isActive;
		#endregion

		#region Properties
		public long Id { get => id; set => id = value; }
		public string Description { get => descritption; set => descritption = value; }
		public string Mrid { get => mrid; set => mrid = value; }
		public string Name { get => name; set => name = value; }
		public long FirstEnd { get => firstEnd; set => firstEnd = value; }
		public List<long> SecondEnd { get => secondEnd; set => secondEnd = value; }
		public string DmsType { get => dmsType; set => dmsType = value; }
		public IMeasurement Measurement { get => measurement; set => measurement = value; }
		public bool IsRemote { get => IsRemote; set => isRemote = value; }
		public bool IsActive { get => isActive; set => isActive = value; }
		#endregion
		public TopologyElement(long gid)
		{
			Id = gid;
			SecondEnd = new List<long>();
		}
	}
}

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

		#endregion

		#region Properties
		public long Id { get => id; set => id = value; }
		public long FirstEnd { get => firstEnd; set => firstEnd = value; }
		public List<long> SecondEnd { get => secondEnd; set => secondEnd = value; }
		public string DmsType { get => dmsType; set => dmsType = value; }
		public IMeasurement Measurement { get => measurement; set => measurement = value; }

		#endregion
		public TopologyElement(long gid)
		{
			Id = gid;
			SecondEnd = new List<long>();
		}
	}
}

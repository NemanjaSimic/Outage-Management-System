using CECommon.Interfaces;
using CECommon.Model;
using System;
using System.Collections.Generic;

namespace CECommon
{
	public class TopologyElement : ITopologyElement
	{
        #region Fields
        private long id;
		private ITopologyElement firstEnd;
		private List<ITopologyElement> secondEnd;
		private string dmsType;
		private List<long> measurements;
		private string descritption;
		private string mrid;
		private string name;
		private float nominalVoltage;
		private bool isRemote;
		private bool isActive;
		private bool noReclosing;
		#endregion

		#region Properties
		public long Id { get => id; set => id = value; }
		public string Description { get => descritption; set => descritption = value; }
		public string Mrid { get => mrid; set => mrid = value; }
		public string Name { get => name; set => name = value; }
		public float NominalVoltage { get => nominalVoltage; set => nominalVoltage = value; }
		public ITopologyElement FirstEnd { get => firstEnd; set => firstEnd = value; }
		public List<ITopologyElement> SecondEnd { get => secondEnd; set => secondEnd = value; }
		public string DmsType { get => dmsType; set => dmsType = value; }
		public List<long> Measurements { get => measurements; set => measurements = value; }
		public bool IsRemote { get => isRemote; set => isRemote = value; }
		public bool IsActive { get => isActive; set => isActive = value; }
		public bool NoReclosing { get => noReclosing; set => noReclosing = value; }

		#endregion
		public TopologyElement(long gid)
		{
			Id = gid;
			SecondEnd = new List<ITopologyElement>();
			Measurements = new List<long>();
		}
	}
}

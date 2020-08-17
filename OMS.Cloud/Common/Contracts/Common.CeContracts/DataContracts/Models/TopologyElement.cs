using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Common.CeContracts
{
    [DataContract(IsReference = true)]
	[KnownType(typeof(EnergyConsumer))]
	[KnownType(typeof(Feeder))]
	[KnownType(typeof(Field))]
	[KnownType(typeof(Recloser))]
	[KnownType(typeof(SynchronousMachine))]
	[KnownType(typeof(TopologyElement))]
	public class TopologyElement : ITopologyElement
	{
		#region Properties
		[DataMember]
		public long Id { get; set; }
		[DataMember]
		public string Description { get; set; }
		[DataMember]
		public string Mrid { get; set; }
		[DataMember]
		public string Name { get; set; }
		[DataMember]
		public float NominalVoltage { get; set; }
		[DataMember]
		public ITopologyElement FirstEnd { get; set; }
		[DataMember]
		public List<ITopologyElement> SecondEnd { get; set; }
		[DataMember]
		public string DmsType { get; set; }
		[DataMember]
		public Dictionary<long, string> Measurements { get; set; }
		[DataMember]
		public bool IsRemote { get; set; }
		[DataMember]
		public bool IsActive { get; set; }
		[DataMember]
		public bool NoReclosing { get; set; }
		[DataMember]
		public ITopologyElement Feeder { get; set; }
		#endregion

		public TopologyElement(long gid)
		{
			Id = gid;
			SecondEnd = new List<ITopologyElement>();
			Measurements = new Dictionary<long, string>();
		}
	}
}

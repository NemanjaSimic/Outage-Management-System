using Common.CE.Interfaces;
using System.Runtime.Serialization;

namespace Common.CeContracts
{
	[DataContract]
    public class SynchronousMachine : TopologyElement
    {
		public SynchronousMachine(ITopologyElement element) : base (element.Id)
		{
            Id = element.Id;
            Description = element.Description;
            Mrid = element.Mrid;
            Name = element.Name;
            NominalVoltage = element.NominalVoltage;
            FirstEnd = element.FirstEnd;
            SecondEnd = element.SecondEnd;
            DmsType = element.DmsType;
            Measurements = element.Measurements;
            IsRemote = element.IsRemote;
            IsActive = element.IsActive;
        }

        [DataMember]
        public float Capacity { get; set; }
        [DataMember]
        public float CurrentRegime { get; set; }
    }
}

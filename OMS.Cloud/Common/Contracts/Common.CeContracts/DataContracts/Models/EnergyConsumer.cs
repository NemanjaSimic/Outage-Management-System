using OMS.Common.Cloud;
using System;
using System.Runtime.Serialization;

namespace Common.CeContracts
{
    [DataContract]
    public class EnergyConsumer : TopologyElement
	{
		public EnergyConsumer(ITopologyElement element) : base (element.Id)
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
        public EnergyConsumerType Type { get; set; }
    }
}

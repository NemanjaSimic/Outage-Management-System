using System;
using System.Runtime.Serialization;

namespace Common.CeContracts
{
    [DataContract]
    public class Recloser : TopologyElement
    {
        [DataMember]
        public int MaxNumberOfTries { get; set; }
        public Recloser(TopologyElement element) : base(element.Id)
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
            NoReclosing = element.NoReclosing;
            NumberOfTry = 0;
            MaxNumberOfTries = 3;
        }
        [DataMember]
        public int NumberOfTry { get; set; }
        public bool IsReachedMaximumOfTries()
        {
            return (NumberOfTry >= MaxNumberOfTries);
        }


    }
}

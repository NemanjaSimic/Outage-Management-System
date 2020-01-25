using System;
using System.Runtime.Serialization;

namespace Outage.Common.UI
{
    [Serializable]
    [DataContract]
    public class UINode
    {
        [DataMember]
        public long Id { get; set; }
        [DataMember]
        public bool IsActive { get; set; }
        [DataMember]
        public float MeasurementValue { get; set; }
        [DataMember]
        public string MeasurementType { get; set; }
        [DataMember]
        public float NominalVoltage { get; set; }
        [DataMember]
        public string DMSType { get; set; }
        [DataMember]
        public bool IsRemote { get; set; }
        public UINode(long id, string type,float nominalVoltage, string measurementType, float measurementValue, bool isActive, bool isRemote)
        {
            Id = id;
            DMSType = type;
            NominalVoltage = nominalVoltage;
            MeasurementType = measurementType;
            MeasurementValue = measurementValue;
            IsActive = isActive;
            IsRemote = isRemote;
        }
    }
}

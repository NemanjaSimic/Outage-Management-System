using System;
using System.Runtime.Serialization;

namespace Outage.Common.UI
{
    [Serializable]
    [DataContract]
    public class UINode
    {
        [DataMember]
        public long Gid { get; set; }
        [DataMember]
        public bool IsActive { get; set; }
        [DataMember]
        public float Measurement { get; set; }
        [DataMember]
        public string MeasurementType { get; set; }
        [DataMember]
        public float NominalVoltage { get; set; }
        [DataMember]
        public string DMSType { get; set; }
        [DataMember]
        public bool IsRemote { get; set; }
        public UINode(long gid, string type,float nominalVoltage, string measurementType, float measurement, bool isActive, bool isRemote)
        {
            Gid = gid;
            DMSType = type;
            NominalVoltage = nominalVoltage;
            MeasurementType = measurementType;
            Measurement = measurement;
            IsActive = isActive;
            IsRemote = isRemote;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Outage.Common.UI
{
    //[Serializable]
    [DataContract]
    public class UINode
    {
        [DataMember]
        public long Id { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string Mrid { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public bool IsActive { get; set; }
        [DataMember]
        public List<Tuple<float, string>> Measurements {get; set;}
        [DataMember]
        public float NominalVoltage { get; set; }
        [DataMember]
        public string DMSType { get; set; }
        [DataMember]
        public bool IsRemote { get; set; }
        public UINode(){}
    }
}

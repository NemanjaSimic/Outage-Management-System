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
        public string DMSType { get; set; }
        [DataMember]
        public ElementType ElementType { get; set; }
        public UINode(long gid, string type)
        {
            Gid = gid;
            DMSType = type;
        }
    }
}

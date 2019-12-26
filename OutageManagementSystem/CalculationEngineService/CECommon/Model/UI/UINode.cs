using System;
using System.Runtime.Serialization;

namespace CECommon.Model.UI
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
        public string Type { get; set; }
        public UINode(long gid, string type)
        {
            Gid = gid;
            Type = type;
        }
    }
}

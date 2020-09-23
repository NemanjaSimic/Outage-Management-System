using OMS.Common.Cloud;
using System.Runtime.Serialization;

namespace OMS.OutageLifecycleImplementation
{
    [DataContract]
    public class PotentialOutageCommand
    {
        [DataMember]
        public long ElementGid { get; set; }
        [DataMember]
        public CommandOriginType CommandOriginType { get; set; }
        [DataMember]
        public NetworkType NetworkType { get; set; }
    }

    [DataContract]
    public enum NetworkType : short
    {
        [EnumMember]
        SCADA_NETWORK = 1,
        [EnumMember]
        NON_SCADA_NETWORK = 2,
    }
}

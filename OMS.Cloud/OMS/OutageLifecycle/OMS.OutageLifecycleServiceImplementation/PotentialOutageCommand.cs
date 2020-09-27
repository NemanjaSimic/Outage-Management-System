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
}

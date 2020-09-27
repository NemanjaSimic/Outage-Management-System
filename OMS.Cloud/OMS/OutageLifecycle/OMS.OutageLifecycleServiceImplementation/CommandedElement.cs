using OMS.Common.Cloud;
using System.Runtime.Serialization;

namespace OMS.OutageLifecycleImplementation
{
    [DataContract]
    public class CommandedElement
    {
        [DataMember]
        public long ElementGid { get; set; }
        [DataMember]
        public long CorrespondingHeadElementGid { get; set; }
        [DataMember]
        public DiscreteCommandingType CommandingType { get; set; }
    }
}

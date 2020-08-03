using System.Runtime.Serialization;

namespace Common.Web.Models
{
    [DataContract]
    public enum OutageLifecycleState
    {
        [EnumMember]
        Unkown = 0,
        [EnumMember]
        Created = 1,
        [EnumMember]
        Isolated = 2,
        [EnumMember]
        Repaired = 3,
        [EnumMember]
        Archived = 4
    }
}

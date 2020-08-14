using System.Runtime.Serialization;

namespace Common.Web.Models
{
    // @TODO:
    // - ovo bi mozda u bazi trebalo da cuvamo ?
    [DataContract]
    public enum ReportType
    {
        [EnumMember]
        Total = 0,
        [EnumMember]
        SAIFI,
        [EnumMember]
        SAIDI
    }
}

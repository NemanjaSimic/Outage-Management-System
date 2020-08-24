using OMS.Common.Cloud;
using System;
using System.Runtime.Serialization;

namespace Common.OmsContracts.DataContracts.Report
{
    [DataContract]
    public class ReportOptions
    {
        [DataMember]
        public ReportType Type { get; set; }
        [DataMember]
        public long? ElementId { get; set; }
        [DataMember]
        public DateTime? StartDate { get; set; }
        [DataMember]
        public DateTime? EndDate { get; set; }
    }
}

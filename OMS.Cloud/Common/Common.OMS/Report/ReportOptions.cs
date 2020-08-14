using OMS.Common.Cloud;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.OMS.Report
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

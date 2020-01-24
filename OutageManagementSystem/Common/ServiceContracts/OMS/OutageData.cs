using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.ServiceContracts.OMS
{
    //[Serializable]
    [DataContract]
    public class OutageData
    {
        [DataMember]
        public long OutageId { get; set; }

        [DataMember]
        public long ElementGid { get; set; }

        [DataMember]
        public DateTime ReportTime { get; set; }

        [DataMember]
        public DateTime? ArchiveTime { get; set; } 

        [DataMember]
        public List<long> AffectedConsumers { get; set; }
    }
}

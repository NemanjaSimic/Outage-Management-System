using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.PubSub.OutageDataContract
{
    [DataContract]
    public abstract class OutageMessage : IPublishableMessage
    {
    }

    [DataContract]
    public class ActiveOutage : OutageMessage
    {
        [Key]
        [DataMember]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long OutageId { get; set; }

        [DataMember]
        public long ElementGid { get; set; }

        [DataMember]
        public DateTime ReportTime { get; set; }

        [DataMember]
        public List<long> AffectedConsumers { get; set; }
    }

    public class ArchivedOutage : OutageMessage
    {
        [Key]
        [DataMember]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long OutageId { get; set; }

        [DataMember]
        public long ElementGid { get; set; }

        [DataMember]
        public DateTime ReportTime { get; set; }

        [DataMember]
        public DateTime ArchiveTime { get; set; }

        [DataMember]
        public List<long> AffectedConsumers { get; set; }
    }
    
}

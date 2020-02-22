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
    [DataContract(IsReference = true)]
    public abstract class OutageMessage : IPublishableMessage
    {
        [DataMember]
        public DateTime ReportTime { get; set; }
        
        [DataMember]
        public string DefaultIsolationPoints { get; set; }

        [DataMember]
        public string OptimumIsolationPoints { get; set; }

        [DataMember]
        public DateTime? IsolatedTime { get; set; }

        [DataMember]
        public DateTime? ResolvedTime { get; set; }

        [DataMember]
        public long OutageElementGid { get; set; }

        [DataMember]
        public OutageState OutageState { get; set; }

        [DataMember]
        public List<Consumer> AffectedConsumers { get; set; }

        public OutageMessage()
        {
            DefaultIsolationPoints = string.Empty;
            OptimumIsolationPoints = string.Empty;
            AffectedConsumers = new List<Consumer>();
        }
    }



    [DataContract(IsReference = true)]
    public class ActiveOutage : OutageMessage
    {
        [Key]
        [DataMember]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long OutageId { get; set; }

        public ActiveOutage() 
            : base()
        {
        }
    }

    [DataContract]
    public class ArchivedOutage : OutageMessage
    {
        [Key]
        [DataMember]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long OutageId { get; set; }

        [DataMember]
        public DateTime ArchiveTime { get; set; }

        public ArchivedOutage()
            : base()
        {
        }
    }

    [DataContract]
    public class Consumer
    {
        [Key]
        [DataMember]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long ConsumerId { get; set; }

        [DataMember]
        public string ConsumerMRID { get; set; }

        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public List<ArchivedOutage> ArchivedOutages { get; set; }

        [DataMember]
        public List<ActiveOutage> ActiveOutages { get; set; }

        public Consumer()
        {
            ConsumerMRID = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            ArchivedOutages = new List<ArchivedOutage>();
            ActiveOutages = new List<ActiveOutage>();
        }
    }
}

using OMS.Common.Cloud;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common.OMS.OutageDatabaseModel
{
    [DataContract]
    public class OutageEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DataMember]
        public long OutageId { get; set; }
        [DataMember]
        public long OutageElementGid { get; set; }
        [DataMember]
        public OutageState OutageState { get; set; }

        [DataMember]
        public DateTime ReportTime { get; set; }
        [DataMember]
        public DateTime? IsolatedTime { get; set; }
        [DataMember]
        public DateTime? RepairedTime { get; set; }
        [DataMember]
        public DateTime? ArchivedTime { get; set; }
        [DataMember]
        public bool IsResolveConditionValidated { get; set; }

        [DataMember]
        public ICollection<Equipment> DefaultIsolationPoints { get; set; }
        [DataMember]
        public ICollection<Equipment> OptimumIsolationPoints { get; set; }
        [DataMember]
        public ICollection<Consumer> AffectedConsumers { get; set; }

        public OutageEntity()
        {
            DefaultIsolationPoints = new List<Equipment>();
            OptimumIsolationPoints = new List<Equipment>();
            AffectedConsumers = new List<Consumer>();
        }
    }
}

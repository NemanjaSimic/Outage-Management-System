using OMS.Common.Cloud;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.OMS.OutageDatabaseModel
{
    public class OutageEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long OutageId { get; set; }
        public long OutageElementGid { get; set; }
        public OutageState OutageState { get; set; }

        public DateTime ReportTime { get; set; }
        public DateTime? IsolatedTime { get; set; }
        public DateTime? RepairedTime { get; set; }
        public DateTime? ArchivedTime { get; set; }

        public bool IsResolveConditionValidated { get; set; }

        public ICollection<Equipment> DefaultIsolationPoints { get; set; }
        public ICollection<Equipment> OptimumIsolationPoints { get; set; }
        public ICollection<Consumer> AffectedConsumers { get; set; }

        public OutageEntity()
        {
            DefaultIsolationPoints = new List<Equipment>();
            OptimumIsolationPoints = new List<Equipment>();
            AffectedConsumers = new List<Consumer>();
        }
    }
}

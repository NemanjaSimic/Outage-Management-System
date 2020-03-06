using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OMSCommon.OutageDatabaseModel
{
    public class Equipment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long EquipmentId { get; set; }
        public string EquipmentMRID { get; set; }
        public ICollection<OutageEntity> OutagesAsOptimumIsolation { get; set; }
        public ICollection<OutageEntity> OutagesAsDefaultIsolation { get; set; }

        public Equipment()
        {
            EquipmentMRID = string.Empty;
            OutagesAsOptimumIsolation = new List<OutageEntity>();
            OutagesAsDefaultIsolation = new List<OutageEntity>();
        }
    }
}

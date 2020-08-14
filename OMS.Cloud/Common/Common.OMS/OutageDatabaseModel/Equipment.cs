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
    public class Equipment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [DataMember]
        public long EquipmentId { get; set; }
        [DataMember]
        public string EquipmentMRID { get; set; }
        [DataMember]
        public ICollection<OutageEntity> OutagesAsOptimumIsolation { get; set; }
        [DataMember]
        public ICollection<OutageEntity> OutagesAsDefaultIsolation { get; set; }

        public Equipment()
        {
            EquipmentMRID = string.Empty;
            OutagesAsOptimumIsolation = new List<OutageEntity>();
            OutagesAsDefaultIsolation = new List<OutageEntity>();
        }
    }
}

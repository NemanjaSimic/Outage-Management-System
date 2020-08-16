using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

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
        public List<OutageEntity> OutagesAsOptimumIsolation { get; set; }

        [DataMember]
        public List<OutageEntity> OutagesAsDefaultIsolation { get; set; }

        public Equipment()
        {
            EquipmentMRID = string.Empty;
            OutagesAsOptimumIsolation = new List<OutageEntity>();
            OutagesAsDefaultIsolation = new List<OutageEntity>();
        }
    }
}

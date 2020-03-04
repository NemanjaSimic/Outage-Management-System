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
        public List<ArchivedOutage> ArchivedOutages { get; set; }
        public List<ActiveOutage> ActiveOutages { get; set; }

        public Equipment()
        {
            EquipmentMRID = string.Empty;
            ArchivedOutages = new List<ArchivedOutage>();
            ActiveOutages = new List<ActiveOutage>();
        }
    }
}

using Outage.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMSCommon.OutageDatabaseModel
{
    public class EquipmentHistorical
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }
        public long EquipmentId { get; set; }
        public long? OutageId { get; set; }
        public DateTime OperationTime { get; set; }
        public DatabaseOperation DatabaseOperation { get; set; }
    }
}

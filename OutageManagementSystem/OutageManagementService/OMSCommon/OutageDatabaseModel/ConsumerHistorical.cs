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
    public class ConsumerHistorical
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }
        public long ConsumerId { get; set; }
        public long? OutageId { get; set; }
        public DateTime OperationTime { get; set; }
        public DatabaseOperation DatabaseOperation { get; set; }

    }
}

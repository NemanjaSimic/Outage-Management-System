using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OMSCommon.OutageDatabaseModel
{
    public class ArchivedOutage : Outage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long OutageId { get; set; }

        public DateTime ArchiveTime { get; set; }

        public ArchivedOutage()
            : base()
        {
        }
    }
}

using Outage.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OMSCommon.OutageDatabaseModel
{
    public class ActiveOutage : Outage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long OutageId { get; set; }

        public ActiveOutageState OutageState { get; set; }

        public ActiveOutage()
            : base()
        {
        }
    }
}

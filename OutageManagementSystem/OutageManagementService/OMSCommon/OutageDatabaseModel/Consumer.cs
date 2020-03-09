using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OMSCommon.OutageDatabaseModel
{
    public class Consumer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long ConsumerId { get; set; }

        public string ConsumerMRID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public ICollection<OutageEntity> Outages { get; set; }

        public Consumer()
        {
            ConsumerMRID = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            Outages = new List<OutageEntity>();
        }
    }
}

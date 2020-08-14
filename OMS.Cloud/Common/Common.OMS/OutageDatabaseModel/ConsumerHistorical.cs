using OMS.Common.Cloud;
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
    public class ConsumerHistorical
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DataMember]
        public long Id { get; set; }
        [DataMember]
        public long ConsumerId { get; set; }
        [DataMember]
        public long? OutageId { get; set; }
        [DataMember]
        public DateTime OperationTime { get; set; }
        [DataMember]
        public DatabaseOperation DatabaseOperation { get; set; }

    }
}

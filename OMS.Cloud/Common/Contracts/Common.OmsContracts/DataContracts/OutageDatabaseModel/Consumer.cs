using OMS.Common.Cloud;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Common.OmsContracts.DataContracts.OutageDatabaseModel
{
    [DataContract(IsReference = true)]
    public class Consumer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [DataMember]
        public long ConsumerId { get; set; }

        [DataMember]
        public string ConsumerMRID { get; set; }

        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public EnergyConsumerType Type { get; set; }

        [DataMember]
        public List<OutageEntity> Outages { get; set; }

        public Consumer()
        {
            ConsumerMRID = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            Outages = new List<OutageEntity>();
        }
    }
    
}

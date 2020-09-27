using OMS.Common.Cloud;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Common.OmsContracts.DataContracts.OutageDatabaseModel
{
	//dodato radi testa 16/8/2020
	[DataContract(IsReference = true)]
    //[DataContract]
    public class OutageEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DataMember]
        public long OutageId { get; set; }

        [DataMember]
        public long OutageElementGid { get; set; }

        [DataMember]
        public OutageState OutageState { get; set; }

        [DataMember]
        public DateTime ReportTime { get; set; }

        [DataMember]
        public DateTime? IsolatedTime { get; set; }

        [DataMember]
        public DateTime? RepairedTime { get; set; }

        [DataMember]
        public DateTime? ArchivedTime { get; set; }

        [DataMember]
        public bool IsResolveConditionValidated { get; set; }

        [DataMember]
        public List<Equipment> DefaultIsolationPoints { get; set; }

        [DataMember]
        public List<Equipment> OptimumIsolationPoints { get; set; }

        [DataMember]
        public List<Consumer> AffectedConsumers { get; set; }

        public OutageEntity()
        {
            DefaultIsolationPoints = new List<Equipment>();
            OptimumIsolationPoints = new List<Equipment>();
            AffectedConsumers = new List<Consumer>();
        }
    }
}

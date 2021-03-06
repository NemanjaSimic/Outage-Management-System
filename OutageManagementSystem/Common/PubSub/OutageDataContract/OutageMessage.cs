﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Outage.Common.PubSub.OutageDataContract
{
    [DataContract(IsReference = true)]
    public abstract class OutageMessage : IPublishableMessage
    {
        [DataMember]
        public DateTime ReportTime { get; set; }
        [DataMember]
        public DateTime? IsolatedTime { get; set; }
        [DataMember]
        public DateTime? RepairedTime { get; set; }

        [DataMember]
        public long OutageElementGid { get; set; }
        [DataMember]
        public IEnumerable<EquipmentMessage> DefaultIsolationPoints { get; set; }
        [DataMember]
        public IEnumerable<EquipmentMessage> OptimumIsolationPoints { get; set; }

        [DataMember]
        public IEnumerable<ConsumerMessage> AffectedConsumers { get; set; }

        public OutageMessage()
        {
            DefaultIsolationPoints = new List<EquipmentMessage>();
            OptimumIsolationPoints = new List<EquipmentMessage>();
            AffectedConsumers = new List<ConsumerMessage>();
        }
    }

    [DataContract(IsReference = true)]
    public class ActiveOutageMessage : OutageMessage
    {
        [DataMember]
        public long OutageId { get; set; }

        [DataMember]
        public OutageState OutageState { get; set; }

        [DataMember]
        public bool IsResolveConditionValidated { get; set; }

        public ActiveOutageMessage() 
            : base()
        {
        }
    }

    [DataContract(IsReference = true)]
    public class ArchivedOutageMessage : OutageMessage
    {
        [DataMember]   
        public long OutageId { get; set; }

        [DataMember]
        public DateTime ArchivedTime { get; set; }

        public ArchivedOutageMessage()
            : base()
        {
        }
    }

    [DataContract]
    public class ConsumerMessage
    {
        [DataMember]
        public long ConsumerId { get; set; }

        [DataMember]
        public string ConsumerMRID { get; set; }
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public IEnumerable<ArchivedOutageMessage> ArchivedOutages { get; set; }
        [DataMember]
        public IEnumerable<ActiveOutageMessage> ActiveOutages { get; set; }

        public ConsumerMessage()
        {
            ConsumerMRID = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            ArchivedOutages = new List<ArchivedOutageMessage>();
            ActiveOutages = new List<ActiveOutageMessage>();
        }
    }

    [DataContract]
    public class EquipmentMessage
    {
        [DataMember]
        public long EquipmentId { get; set; }
        [DataMember]
        public string EquipmentMRID { get; set; }

        [DataMember]
        public IEnumerable<ArchivedOutageMessage> ArchivedOutages { get; set; }
        [DataMember]
        public IEnumerable<ActiveOutageMessage> ActiveOutages { get; set; }

        public EquipmentMessage()
        {
            EquipmentMRID = string.Empty;
            ArchivedOutages = new List<ArchivedOutageMessage>();
            ActiveOutages = new List<ActiveOutageMessage>();
        }
    }

    //[DataContract]
    //public class ConsumerHistoricalMessage
    //{
    //    [DataMember]
    //    public long Id { get; set; }
    //    [DataMember]
    //    public long ConsumerId { get; set; }
    //    [DataMember]
    //    public long? OutageId { get; set; }
    //    [DataMember]
    //    public DateTime OperationTime { get; set; }
    //    [DataMember]
    //    public DatabaseOperation DatabaseOperation { get; set; }
    //}

    //[DataContract]
    //public class EquipmentHistoricalMessage
    //{
    //    [DataMember]
    //    public long Id { get; set; }
    //    [DataMember]
    //    public long EquipmentId { get; set; }
    //    [DataMember]
    //    public long? OutageId { get; set; }
    //    [DataMember]
    //    public DateTime OperationTime { get; set; }
    //    [DataMember]
    //    public DatabaseOperation DatabaseOperation { get; set; }
    //}
}

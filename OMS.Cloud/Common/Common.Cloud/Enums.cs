using System.Runtime.Serialization;

namespace OMS.Common.Cloud
{
    //NMS
    [DataContract]
    public enum AnalogMeasurementType : short
    {
        [EnumMember]
        VOLTAGE = 1,

        [EnumMember]
        CURRENT = 2,

        [EnumMember]
        POWER = 3,
    }

    [DataContract]
    public enum DiscreteMeasurementType : short
    {
        [EnumMember]
        SWITCH_STATUS = 1,
    }


    //PUB_SUB
    [DataContract]
    public enum Topic : short
    {
        [EnumMember]
        MEASUREMENT = 1,

        [EnumMember]
        SWITCH_STATUS = 2,

        [EnumMember]
        TOPOLOGY = 3,

        [EnumMember]
        OUTAGE_EMAIL = 4,

        [EnumMember]
        ACTIVE_OUTAGE = 5,

        [EnumMember]
        ARCHIVED_OUTAGE = 6,

        [EnumMember]
        OMS_MODEL = 7,
    }

    [DataContract]
    public enum ServiceType
    {
        [EnumMember]
        STATEFUL_SERVICE = 1,

        [EnumMember]
        STATELESS_SERVICE = 2,
    }

    //SCADA
    [DataContract]
    public enum AlarmType : short
    {
        [EnumMember]
        NO_ALARM = 0x01,

        [EnumMember]
        REASONABILITY_FAILURE = 0x02,

        [EnumMember]
        LOW_ALARM = 0x03,

        [EnumMember]
        HIGH_ALARM = 0x04,

        [EnumMember]
        ABNORMAL_VALUE = 0x05,
    }

    [DataContract]
    public enum DiscreteCommandingType : ushort
    {
        [EnumMember]
        CLOSE = 0x00,

        [EnumMember]
        OPEN = 0x01,
    }

    [DataContract]
    public enum ElementType
    {

        [EnumMember]
        Remote = 1,

        [EnumMember]
        Local
    }

    [DataContract]
    public enum CommandOriginType : short
    {
        [EnumMember]
        USER_COMMAND = 0x1,

        [EnumMember]
        ISOLATING_ALGORITHM_COMMAND,

        [EnumMember]
        CE_COMMAND,

        [EnumMember]
        MODEL_UPDATE_COMMAND,

        [EnumMember]
        OUTAGE_SIMULATOR,

        [EnumMember]
        OTHER_COMMAND, //TODO: rethink of name, add others like CE ili tako nesto
    }

    //OMS
    [DataContract]
    public enum OutageState : short
    {
        [EnumMember]
        CREATED = 1,

        [EnumMember]
        ISOLATED = 2,

        [EnumMember]
        REPAIRED = 3,

        [EnumMember]
        ARCHIVED = 4,

        [EnumMember]
        REMOVED = 5
    }
    [DataContract]
    public enum DatabaseOperation : short
    {
        [EnumMember]
        INSERT = 1,
        [EnumMember]
        DELETE = 2
    }

    [DataContract]
    public enum ModelUpdateOperationType : short
    {
        [EnumMember]
        INSERT = 1,
        [EnumMember]
        DELETE = 2,
        [EnumMember]
        CLEAR = 3
    }

    public enum ReportType
    {
        Total = 0,
        SAIFI,
        SAIDI
    }
}


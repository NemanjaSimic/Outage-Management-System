using System.Runtime.Serialization;

namespace Outage.Common
{
    //NMS
    public enum AnalogMeasurementType : short
    {
        VOLTAGE = 1,
        CURRENT = 2,
        POWER   = 3,
    }

    public enum DiscreteMeasurementType : short
    {
        SWITCH_STATUS   = 1,
    }


    //PUB_SUB
    [DataContract]
    public enum Topic
    {
        [EnumMember]
        MEASUREMENT = 1,

        [EnumMember]
        SWITCH_STATUS,

        [EnumMember]
        TOPOLOGY,

        [EnumMember]
        OUTAGE_EMAIL,

        [EnumMember]
        ACTIVE_OUTAGE,

        [EnumMember]
        ARCHIVED_OUTAGE,

        [EnumMember]
        OMS_MODEL
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
    public enum ActiveOutageState : short
    {
        [EnumMember]
        CREATED = 1,
        [EnumMember]
        ISOLATED = 2,
        [EnumMember]
        RESOLVED = 3,
        //[EnumMember]
        //ARCHIVED = 4
    }
}

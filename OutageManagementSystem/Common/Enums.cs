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
        MEASUREMENT = 0,

        [EnumMember]
        SWITCH_STATUS
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
}

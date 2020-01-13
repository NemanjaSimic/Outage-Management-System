using System.Runtime.Serialization;

namespace Outage.Common
{
    //NMS
    public enum AnalogMeasurementType : short
    {

    }

    public enum DiscreteMeasurementType : short
    {

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
}

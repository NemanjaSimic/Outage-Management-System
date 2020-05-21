using System.Runtime.Serialization;

namespace OMS.Common.SCADA
{
    [DataContract]
    public enum ModbusFunctionCode : short
    {
        [EnumMember]
        READ_COILS                  = 0x01,
        
        [EnumMember]
        READ_DISCRETE_INPUTS        = 0x02,
        
        [EnumMember]
        READ_HOLDING_REGISTERS      = 0x03,
        
        [EnumMember]
        READ_INPUT_REGISTERS        = 0x04,
        
        [EnumMember]
        WRITE_SINGLE_COIL           = 0x05,
        
        [EnumMember]
        WRITE_SINGLE_REGISTER       = 0x06,
        
        [EnumMember]
        WRITE_MULTIPLE_COILS        = 0x07,
        
        [EnumMember]
        WRITE_MULTIPLE_REGISTERS    = 0x08,
    }

    [DataContract]
    public enum PointType : short
    {
        [EnumMember]
        DIGITAL_OUTPUT  = 0x01,

        [EnumMember]
        DIGITAL_INPUT   = 0x02,

        [EnumMember]
        ANALOG_INPUT    = 0x03,

        [EnumMember]
        ANALOG_OUTPUT   = 0x04,

        [EnumMember]
        HR_LONG         = 0x05,
    }
}
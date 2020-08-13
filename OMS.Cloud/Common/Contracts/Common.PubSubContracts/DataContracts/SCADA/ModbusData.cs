using OMS.Common.Cloud;
using OMS.Common.PubSubContracts.Interfaces;
using System.Runtime.Serialization;

namespace OMS.Common.PubSubContracts.DataContracts.SCADA
{
    [DataContract]
    [KnownType(typeof(AnalogModbusData))]
    [KnownType(typeof(DiscreteModbusData))]
    public abstract class ModbusData : IModbusData
    {
        [DataMember]
        public long MeasurementGid { get; protected set; }

        [DataMember]
        public AlarmType Alarm { get; protected set; }

        [DataMember]
        public CommandOriginType CommandOrigin { get; protected set; }
    }

    [DataContract]
    public class AnalogModbusData : ModbusData
    {
        public AnalogModbusData(float value, AlarmType alarm, long measurementGid, CommandOriginType commandOrigin)
        {
            MeasurementGid = measurementGid;
            Value = value;
            Alarm = alarm;
            CommandOrigin = commandOrigin;
        }

        [DataMember]
        public float Value { get; private set; }
    }

    [DataContract]
    public class DiscreteModbusData : ModbusData
    {
        public DiscreteModbusData(ushort value, AlarmType alarm, long measurementGid, CommandOriginType commandOrigin)
        {
            MeasurementGid = measurementGid;
            Value = value;
            Alarm = alarm;
            CommandOrigin = commandOrigin;
        }

        [DataMember]
        public ushort Value { get; private set; }
    }
}

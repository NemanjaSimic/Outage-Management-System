using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.PubSub.SCADADataContract
{
    [DataContract]
    public class AnalogModbusData : IModbusData
    {
        public AnalogModbusData(float value, AlarmType alarm)
        {
            Value = value;
            Alarm = alarm;
        }

        [DataMember]
        public float Value { get; private set; }

        [DataMember]
        public AlarmType Alarm { get; private set; }
    }

    [DataContract]
    public class DiscreteModbusData : IModbusData
    {
        public DiscreteModbusData(ushort value, AlarmType alarm)
        {
            Value = value;
            Alarm = alarm;
        }

        [DataMember]
        public ushort Value { get; private set; }

        [DataMember]
        public AlarmType Alarm { get; private set; }

    }
}

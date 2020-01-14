using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.PubSub.SCADADataContract
{
    //[Serializable]
    [DataContract]
    public class AnalogModbusData : IModbusData
    {
        public AnalogModbusData(double value, AlarmType alarm)
        {
            Value = value;
            Alarm = alarm;
        }

        [DataMember]
        public double Value { get; private set; }

        [DataMember]
        public AlarmType Alarm { get; private set; }
    }

    //[Serializable]
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

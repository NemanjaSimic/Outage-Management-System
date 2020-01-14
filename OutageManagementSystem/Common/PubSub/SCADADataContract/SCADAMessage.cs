using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Outage.Common.PubSub.SCADADataContract
{
    [DataContract]
    public abstract class SCADAMessage : IPublishableMessage
    {
    }

    [Serializable]
    [DataContract]
    public class SingleAnalogValueSCADAMessage : SCADAMessage
    {
        public SingleAnalogValueSCADAMessage(long gid, float value, AlarmType alarm)
        {
            Gid = gid;
            Value = value;
            Alarm = alarm;
        }

        [DataMember]
        public long Gid { get; private set; }

        [DataMember]
        public float Value { get; private set; }

        [DataMember]
        public AlarmType Alarm { get; private set; }
    }

    [Serializable]
    [DataContract]
    public class MultipleAnalogValueSCADAMessage : SCADAMessage
    {
        public MultipleAnalogValueSCADAMessage(Dictionary<long, AnalogModbusData> data)
        {
            Data = data;
        }

        [DataMember]
        public Dictionary<long, AnalogModbusData> Data { get; private set; }
    }

    [Serializable]
    [DataContract]
    public class SingleDiscreteValueSCADAMessage : SCADAMessage
    {
        public SingleDiscreteValueSCADAMessage(long gid, ushort value, AlarmType alarm)
        {
            Gid = gid;
            Value = value;
            Alarm = alarm;
        }

        public long Gid { get; private set; }

        public ushort Value { get; private set; }

        [DataMember]
        public AlarmType Alarm { get; private set; }
    }

    [Serializable]
    [DataContract]
    public class MultipleDiscreteValueSCADAMessage : SCADAMessage
    {
        public MultipleDiscreteValueSCADAMessage(Dictionary<long, DiscreteModbusData> data)
        {
            Data = data;
        }

        [DataMember]
        public Dictionary<long, DiscreteModbusData> Data { get; private set; }
    }
}

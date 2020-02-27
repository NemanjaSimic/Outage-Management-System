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

    [DataContract]
    public class SingleAnalogValueSCADAMessage : SCADAMessage
    {
        public SingleAnalogValueSCADAMessage(AnalogModbusData analogModbusData)
        {
            AnalogModbusData = analogModbusData;
        }

        [DataMember]
        public AnalogModbusData AnalogModbusData { get; private set; }
    }

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

    [DataContract]
    public class SingleDiscreteValueSCADAMessage : SCADAMessage
    {
        public SingleDiscreteValueSCADAMessage(DiscreteModbusData discreteModbusData)
        {
            DiscreteModbusData = discreteModbusData;
        }

        [DataMember]
        public DiscreteModbusData DiscreteModbusData { get; private set; }
    }

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

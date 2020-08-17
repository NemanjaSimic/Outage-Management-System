using OMS.Common.PubSubContracts.Interfaces;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OMS.Common.PubSubContracts.DataContracts.SCADA
{
    [DataContract]
    [KnownType(typeof(SingleAnalogValueSCADAMessage))]
    [KnownType(typeof(MultipleAnalogValueSCADAMessage))]
    [KnownType(typeof(SingleDiscreteValueSCADAMessage))]
    [KnownType(typeof(MultipleDiscreteValueSCADAMessage))]
    public abstract class ScadaMessage : IPublishableMessage
    {
    }

    [DataContract]
    public class SingleAnalogValueSCADAMessage : ScadaMessage
    {
        public SingleAnalogValueSCADAMessage(AnalogModbusData analogModbusData)
        {
            AnalogModbusData = analogModbusData;
        }

        [DataMember]
        public AnalogModbusData AnalogModbusData { get; private set; }
    }

    [DataContract]
    public class MultipleAnalogValueSCADAMessage : ScadaMessage
    {
        public MultipleAnalogValueSCADAMessage(Dictionary<long, AnalogModbusData> data)
        {
            Data = data;
        }

        [DataMember]
        public Dictionary<long, AnalogModbusData> Data { get; private set; }
    }

    [DataContract]
    public class SingleDiscreteValueSCADAMessage : ScadaMessage
    {
        public SingleDiscreteValueSCADAMessage(DiscreteModbusData discreteModbusData)
        {
            DiscreteModbusData = discreteModbusData;
        }

        [DataMember]
        public DiscreteModbusData DiscreteModbusData { get; private set; }
    }

    [DataContract]
    public class MultipleDiscreteValueSCADAMessage : ScadaMessage
    {
        public MultipleDiscreteValueSCADAMessage(Dictionary<long, DiscreteModbusData> data)
        {
            Data = data;
        }

        [DataMember]
        public Dictionary<long, DiscreteModbusData> Data { get; private set; }
    }
}

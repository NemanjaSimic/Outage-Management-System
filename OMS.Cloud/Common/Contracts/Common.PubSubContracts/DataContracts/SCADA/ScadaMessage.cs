using Common.PubSub;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace OMS.Common.PubSubContracts.DataContracts.SCADA
{
    [DataContract]
    [ServiceKnownType(typeof(SingleAnalogValueSCADAMessage))]
    [ServiceKnownType(typeof(MultipleAnalogValueSCADAMessage))]
    [ServiceKnownType(typeof(SingleDiscreteValueSCADAMessage))]
    [ServiceKnownType(typeof(MultipleDiscreteValueSCADAMessage))]
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

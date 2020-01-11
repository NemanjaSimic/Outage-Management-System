using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Outage.Common.PubSub.SCADADataContract
{
    [Serializable]
    [DataContract]
    [KnownType(typeof(SingleAnalogValueSCADAMessage))]
    [KnownType(typeof(MultipleAnalogValueSCADAMessage))]
    [KnownType(typeof(SingleDiscreteValueSCADAMessage))]
    [KnownType(typeof(MultipleDiscreteValueSCADAMessage))]
    //[KnownType(typeof(IPublishableMessage))]
    public abstract class SCADAMessage : IPublishableMessage
    {
    }

    [Serializable]
    [DataContract]
    //[KnownType(typeof(SCADAMessage))]
    //[KnownType(typeof(IPublishableMessage))]
    public class SingleAnalogValueSCADAMessage : SCADAMessage
    {
        public SingleAnalogValueSCADAMessage(long gid, int value)
        {
            Gid = gid;
            Value = value;
        }

        [DataMember]
        public long Gid { get; private set; }

        [DataMember]
        public int Value { get; private set; }
    }

    [Serializable]
    [DataContract]
    //[KnownType(typeof(SCADAMessage))]
    //[KnownType(typeof(IPublishableMessage))]
    public class MultipleAnalogValueSCADAMessage : SCADAMessage
    {
        public MultipleAnalogValueSCADAMessage(Dictionary<long, int> data)
        {
            Data = data;
        }

        [DataMember]
        public Dictionary<long, int> Data { get; private set; }
    }

    [Serializable]
    [DataContract]
    //[KnownType(typeof(SCADAMessage))]
    //[KnownType(typeof(IPublishableMessage))]
    public class SingleDiscreteValueSCADAMessage : SCADAMessage
    {
        public SingleDiscreteValueSCADAMessage(long gid, bool value)
        {
            Gid = gid;
            Value = value;
        }

        [DataMember]
        public long Gid { get; private set; }

        [DataMember]
        public bool Value { get; private set; }
    }

    [Serializable]
    [DataContract]
    //[KnownType(typeof(SCADAMessage))]
    //[KnownType(typeof(IPublishableMessage))]
    public class MultipleDiscreteValueSCADAMessage : SCADAMessage
    {
        public MultipleDiscreteValueSCADAMessage(Dictionary<long, bool> data)
        {
            Data = data;
        }

        [DataMember]
        public Dictionary<long, bool> Data { get; private set; }
    }
}

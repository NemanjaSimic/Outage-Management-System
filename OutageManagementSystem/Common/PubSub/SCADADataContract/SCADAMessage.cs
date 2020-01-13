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
        public SingleAnalogValueSCADAMessage(long gid, int value)
        {
            Gid = gid;
            Value = value;
        }

        public long Gid { get; private set; }

        public int Value { get; private set; }
    }

    [Serializable]
    [DataContract]
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
    public class SingleDiscreteValueSCADAMessage : SCADAMessage
    {
        public SingleDiscreteValueSCADAMessage(long gid, bool value)
        {
            Gid = gid;
            Value = value;
        }

        public long Gid { get; private set; }

        public bool Value { get; private set; }
    }

    [Serializable]
    [DataContract]
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

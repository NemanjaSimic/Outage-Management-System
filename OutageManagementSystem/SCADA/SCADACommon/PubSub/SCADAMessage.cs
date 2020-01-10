using Outage.Common.PubSub;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Outage.SCADA.SCADACommon.PubSub
{
    [DataContract]
    public class SingleAnalogValueSCADAMessage : ISingleAnalogValueSCADAMessage
    {
        [DataMember]
        public long Gid { get; set; }

        [DataMember]
        public int Value { get; set; }
    }

    [DataContract]
    public class MultipleAnalogValueSCADAMessage : IMultipleAnalogValueSCADAMessage
    {
        [DataMember]
        public Dictionary<long, int> Values { get; set; }
    }

    public class SingleDiscreteValueSCADAMessage : ISingleDiscreteValueSCADAMessage
    {
        [DataMember]
        public long Gid { get; set; }

        [DataMember]
        public bool Value { get; set; }
    }

    [DataContract]
    public class MultipleDiscreteValueSCADAMessage : IMultipleDiscreteValueSCADAMessage
    {
        [DataMember]
        public Dictionary<long, bool> Values { get; set; }
    }
}
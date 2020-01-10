using Outage.Common.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Outage.SCADA.SCADACommon.PubSub
{
    //TODO: scada message types

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

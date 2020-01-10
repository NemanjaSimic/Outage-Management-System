using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.PubSub
{
    public interface IPublishableMessage
    {
    }

    public interface ISingleAnalogValueSCADAMessage : IPublishableMessage
    {
        [DataMember]
        long Gid { get; }
        [DataMember]
        int Value { get; }
    }

    public interface IMultipleAnalogValueSCADAMessage : IPublishableMessage
    {
        [DataMember]
        Dictionary<long, int> Values { get; }
    }

    public interface ISingleDiscreteValueSCADAMessage : IPublishableMessage
    {
        [DataMember]
        long Gid { get; }
        [DataMember]
        bool Value { get; }
    }

    public interface IMultipleDiscreteValueSCADAMessage : IPublishableMessage
    {
        [DataMember]
        Dictionary<long, bool> Values { get; }
    }

    public interface ICalculationEngineMessage : IPublishableMessage
    {
    }
}

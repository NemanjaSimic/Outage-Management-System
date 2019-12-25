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

    public interface ISCADAMessage : IPublishableMessage
    {
        [DataMember]
        long Gid { get; }
        [DataMember]
        object Value { get; }
    }

    public interface ICalculationEngineMessage : IPublishableMessage
    {
    }
}

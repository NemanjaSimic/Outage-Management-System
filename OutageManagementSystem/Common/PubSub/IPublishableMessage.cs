using System.Runtime.Serialization;

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

using Common.PubSub;
using OMS.Common.Cloud;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using System.Runtime.Serialization;

namespace OMS.Common.PubSubContracts.DataContracts
{
    [DataContract]
    [KnownType(typeof(ScadaPublication))]
    public abstract class Publication : IPublication
    {
        protected Publication(Topic topic, IPublishableMessage message)
        {
            Topic = topic;
            Message = message;
        }

        [DataMember]
        public Topic Topic { get; private set; }

        [DataMember]
        public IPublishableMessage Message { get; private set; }
    }
}

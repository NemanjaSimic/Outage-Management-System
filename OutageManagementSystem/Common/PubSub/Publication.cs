using System;
using System.Runtime.Serialization;
using Outage.Common.PubSub.SCADADataContract;
using Outage.Common.PubSub.CalculationEngineDataContract;

namespace Outage.Common.PubSub
{
    [Serializable]
    [DataContract]
    [KnownType(typeof(SCADAPublication))]
    //[KnownType(typeof(IPublication))]
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

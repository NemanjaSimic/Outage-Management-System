using Common.PubSubContracts.DataContracts.CE;
using Common.PubSubContracts.DataContracts.EMAIL;
using Common.PubSubContracts.DataContracts.OMS;
using OMS.Common.Cloud;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using OMS.Common.PubSubContracts.Interfaces;
using System.Runtime.Serialization;

namespace OMS.Common.PubSubContracts.DataContracts
{
    [DataContract]
    [KnownType(typeof(ScadaPublication))]
    [KnownType(typeof(OutagePublication))]
    [KnownType(typeof(OutageEmailPublication))]
    [KnownType(typeof(CalculationEnginePublication))]
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

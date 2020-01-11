using Outage.Common.PubSub;
using System.ServiceModel;
using Outage.Common.PubSub.SCADADataContract;

namespace Outage.Common.ServiceContracts.PubSub
{
    [ServiceContract]
    public interface IPublisher
    {
        [OperationContract]
        [ServiceKnownType(typeof(Publication))]
        [ServiceKnownType(typeof(SCADAPublication))]
        [ServiceKnownType(typeof(SCADAMessage))]
        [ServiceKnownType(typeof(SingleAnalogValueSCADAMessage))]
        [ServiceKnownType(typeof(MultipleAnalogValueSCADAMessage))]
        [ServiceKnownType(typeof(SingleDiscreteValueSCADAMessage))]
        [ServiceKnownType(typeof(MultipleDiscreteValueSCADAMessage))]
        void Publish(IPublication publication);
    }
}
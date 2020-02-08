using Outage.Common.OutageService.Model;
using Outage.Common.PubSub;
using Outage.Common.PubSub.CalculationEngineDataContract;
using Outage.Common.PubSub.EmailDataContract;
using Outage.Common.PubSub.OutageDataContract;
using Outage.Common.PubSub.SCADADataContract;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Outage.Common.ServiceContracts.PubSub
{
    [ServiceContract(CallbackContract = typeof(ISubscriberCallback))]
    public interface ISubscriber
    {
        [OperationContract(IsOneWay = true)]
        void Subscribe(Topic topic);
    }

    public interface ISubscriberCallback
    {
        [OperationContract]
        string GetSubscriberName();

        [OperationContract(IsOneWay = true)]
        [ServiceKnownType(typeof(SCADAMessage))]
        [ServiceKnownType(typeof(SingleAnalogValueSCADAMessage))]
        [ServiceKnownType(typeof(MultipleAnalogValueSCADAMessage))]
        [ServiceKnownType(typeof(SingleDiscreteValueSCADAMessage))]
        [ServiceKnownType(typeof(MultipleDiscreteValueSCADAMessage))]
        [ServiceKnownType(typeof(CalculationEngineMessage))]
        [ServiceKnownType(typeof(CalculationEnginePublication))]
        [ServiceKnownType(typeof(TopologyForUIMessage))]
        [ServiceKnownType(typeof(EmailServiceMessage))]
        [ServiceKnownType(typeof(EmailToOutageMessage))]
        [ServiceKnownType(typeof(OMSModelMessage))]
        [ServiceKnownType(typeof(ActiveOutage))]
        [ServiceKnownType(typeof(ArchivedOutage))]
        [ServiceKnownType(typeof(OutageMessage))]
        [ServiceKnownType(typeof(OutageTopologyModel))]
        [ServiceKnownType(typeof(OutageTopologyElement))]
        void Notify(IPublishableMessage message);
    }
}
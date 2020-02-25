using Outage.Common.PubSub;
using System.ServiceModel;
using Outage.Common.PubSub.SCADADataContract;
using Outage.Common.PubSub.CalculationEngineDataContract;
using Outage.Common.PubSub.EmailDataContract;
using Outage.Common.PubSub.OutageDataContract;
using Outage.Common.OutageService.Model;

namespace Outage.Common.ServiceContracts.PubSub
{
    [ServiceContract]
    public interface IPublisher
    {
        [OperationContract]
        [ServiceKnownType(typeof(Publication))]
        [ServiceKnownType(typeof(SCADAPublication))]
        [ServiceKnownType(typeof(SCADAMessage))]                        //TODO: delete?
        [ServiceKnownType(typeof(SingleAnalogValueSCADAMessage))]
        [ServiceKnownType(typeof(MultipleAnalogValueSCADAMessage))]
        [ServiceKnownType(typeof(SingleDiscreteValueSCADAMessage))]
        [ServiceKnownType(typeof(MultipleDiscreteValueSCADAMessage))]
        [ServiceKnownType(typeof(CalculationEngineMessage))]
        [ServiceKnownType(typeof(CalculationEnginePublication))]
        [ServiceKnownType(typeof(TopologyForUIMessage))]    
        [ServiceKnownType(typeof(OutageEmailPublication))]
        [ServiceKnownType(typeof(EmailServiceMessage))]
        [ServiceKnownType(typeof(EmailToOutageMessage))]
        [ServiceKnownType(typeof(OMSModelMessage))]
        [ServiceKnownType(typeof(OutagePublication))]
        [ServiceKnownType(typeof(OutageMessage))]                       //TODO: delete?
        [ServiceKnownType(typeof(OutageTopologyModel))]
        [ServiceKnownType(typeof(OutageTopologyElement))]
        [ServiceKnownType(typeof(ActiveOutageMessage))]
        [ServiceKnownType(typeof(ArchivedOutageMessage))]
        [ServiceKnownType(typeof(ConsumerMessage))]
        void Publish(IPublication publication, string publisherName);
    }
}
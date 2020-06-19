using Common.PubSub;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace OMS.Common.PubSubContracts
{
    [ServiceContract]
    public interface IPublisherContract : IService
    {
        [OperationContract]
        //[ServiceKnownType(typeof(CalculationEngineMessage))]
        //[ServiceKnownType(typeof(CalculationEnginePublication))]
        //[ServiceKnownType(typeof(TopologyForUIMessage))]
        //[ServiceKnownType(typeof(OutageEmailPublication))]
        //[ServiceKnownType(typeof(EmailServiceMessage))]
        //[ServiceKnownType(typeof(EmailToOutageMessage))]
        //[ServiceKnownType(typeof(OMSModelMessage))]
        //[ServiceKnownType(typeof(OutagePublication))]
        //[ServiceKnownType(typeof(OutageTopologyModel))]
        //[ServiceKnownType(typeof(OutageTopologyElement))]
        //[ServiceKnownType(typeof(ActiveOutageMessage))]
        //[ServiceKnownType(typeof(ArchivedOutageMessage))]
        //[ServiceKnownType(typeof(ConsumerMessage))]
        [ServiceKnownType(typeof(ScadaPublication))]
        [ServiceKnownType(typeof(SingleAnalogValueSCADAMessage))]
        [ServiceKnownType(typeof(MultipleAnalogValueSCADAMessage))]
        [ServiceKnownType(typeof(SingleDiscreteValueSCADAMessage))]
        [ServiceKnownType(typeof(MultipleDiscreteValueSCADAMessage))]
        Task<bool> Publish(IPublication publication, Uri publisherUri);
    }
}

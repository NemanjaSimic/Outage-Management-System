using OMS.Common.PubSubContracts.DataContracts.SCADA;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;
using System.Threading.Tasks;
using Common.PubSubContracts.DataContracts.EMAIL;
using Common.CloudContracts;
using OMS.Common.PubSubContracts.Interfaces;

namespace OMS.Common.PubSubContracts
{
    [ServiceContract]
    public interface INotifySubscriberContract: IService, IHealthChecker
    {
        [OperationContract]
        //[ServiceKnownType(typeof(CalculationEngineMessage))]
        //[ServiceKnownType(typeof(CalculationEnginePublication))]
        //[ServiceKnownType(typeof(TopologyForUIMessage))]
        [ServiceKnownType(typeof(EmailServiceMessage))]
        [ServiceKnownType(typeof(EmailToOutageMessage))]
        //[ServiceKnownType(typeof(OMSModelMessage))]
        //[ServiceKnownType(typeof(ActiveOutageMessage))]
        //[ServiceKnownType(typeof(ArchivedOutageMessage))]
        //[ServiceKnownType(typeof(OutageTopologyModel))]
        //[ServiceKnownType(typeof(OutageTopologyElement))]  
        [ServiceKnownType(typeof(SingleAnalogValueSCADAMessage))]
        [ServiceKnownType(typeof(MultipleAnalogValueSCADAMessage))]
        [ServiceKnownType(typeof(SingleDiscreteValueSCADAMessage))]
        [ServiceKnownType(typeof(MultipleDiscreteValueSCADAMessage))]

        Task Notify(IPublishableMessage message, string publisherName);

        [OperationContract]
        Task<string> GetSubscriberName();
    }
}
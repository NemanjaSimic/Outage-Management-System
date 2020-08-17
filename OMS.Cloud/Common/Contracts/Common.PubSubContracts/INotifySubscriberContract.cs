using OMS.Common.PubSubContracts.DataContracts.SCADA;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;
using System.Threading.Tasks;
using Common.PubSubContracts.DataContracts.EMAIL;
using Common.CloudContracts;
using OMS.Common.PubSubContracts.Interfaces;
using Common.PubSubContracts.DataContracts.CE;
using Common.PubSubContracts.DataContracts.OMS;

namespace OMS.Common.PubSubContracts
{
    [ServiceContract]
    public interface INotifySubscriberContract: IService, IHealthChecker
    {
        [OperationContract]
        //SCADA
        [ServiceKnownType(typeof(SingleAnalogValueSCADAMessage))]
        [ServiceKnownType(typeof(MultipleAnalogValueSCADAMessage))]
        [ServiceKnownType(typeof(SingleDiscreteValueSCADAMessage))]
        [ServiceKnownType(typeof(MultipleDiscreteValueSCADAMessage))]
        //OMS
        [ServiceKnownType(typeof(ActiveOutageMessage))]
        [ServiceKnownType(typeof(ArchivedOutageMessage))]
        [ServiceKnownType(typeof(ConsumerMessage))]
        [ServiceKnownType(typeof(EquipmentMessage))]
        //EMAIL
        [ServiceKnownType(typeof(EmailServiceMessage))]
        [ServiceKnownType(typeof(EmailToOutageMessage))]
        //CE
        [ServiceKnownType(typeof(CalculationEngineMessage))]
        [ServiceKnownType(typeof(TopologyForUIMessage))]
        [ServiceKnownType(typeof(OMSModelMessage))]
        [ServiceKnownType(typeof(OutageTopologyModel))]
        [ServiceKnownType(typeof(OutageTopologyElement))]
        Task Notify(IPublishableMessage message, string publisherName);

        [OperationContract]
        Task<string> GetSubscriberName();
    }
}
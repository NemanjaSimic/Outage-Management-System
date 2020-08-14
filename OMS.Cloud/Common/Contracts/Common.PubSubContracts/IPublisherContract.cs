using OMS.Common.PubSubContracts.DataContracts.SCADA;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;
using System.Threading.Tasks;
using Common.PubSubContracts.DataContracts.OMS;
using Common.CloudContracts;
using OMS.Common.PubSubContracts.Interfaces;
using Common.PubSubContracts.DataContracts.EMAIL;
using Common.PubSubContracts.DataContracts.CE;

namespace OMS.Common.PubSubContracts
{
    [ServiceContract]
    public interface IPublisherContract : IService, IHealthChecker
    {
        [OperationContract]
        //SCADA
        [ServiceKnownType(typeof(ScadaPublication))]
        [ServiceKnownType(typeof(SingleAnalogValueSCADAMessage))]
        [ServiceKnownType(typeof(MultipleAnalogValueSCADAMessage))]
        [ServiceKnownType(typeof(SingleDiscreteValueSCADAMessage))]
        [ServiceKnownType(typeof(MultipleDiscreteValueSCADAMessage))]
        //OMS
        [ServiceKnownType(typeof(OutagePublication))]
        [ServiceKnownType(typeof(ActiveOutageMessage))]
        [ServiceKnownType(typeof(ArchivedOutageMessage))]
        [ServiceKnownType(typeof(ConsumerMessage))]
        [ServiceKnownType(typeof(EquipmentMessage))]
        //EMAIL
        [ServiceKnownType(typeof(OutageEmailPublication))]
        [ServiceKnownType(typeof(EmailServiceMessage))]
        [ServiceKnownType(typeof(EmailToOutageMessage))]
        //CE
        [ServiceKnownType(typeof(CalculationEnginePublication))]
        [ServiceKnownType(typeof(CalculationEngineMessage))]
        [ServiceKnownType(typeof(TopologyForUIMessage))]
        [ServiceKnownType(typeof(OMSModelMessage))]
        [ServiceKnownType(typeof(OutageTopologyModel))]
        [ServiceKnownType(typeof(OutageTopologyElement))]
        Task<bool> Publish(IPublication publication, string publisherUri);
    }
}

using Outage.Common.PubSub;
using System.ServiceModel;
using Outage.Common.PubSub.SCADADataContract;
using Outage.Common.PubSub.CalculationEngineDataContract;
using Outage.Common.PubSub.EmailDataContract;

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
        [ServiceKnownType(typeof(CalculationEngineMessage))]
        [ServiceKnownType(typeof(CalcualtionEnginePublication))]
        [ServiceKnownType(typeof(TopologyForUIMessage))]    
        [ServiceKnownType(typeof(OutageEmailPublication))]
        [ServiceKnownType(typeof(EmailServiceMessage))]
        [ServiceKnownType(typeof(EmailToOutageMessage))]
        [ServiceKnownType(typeof(OMSModelMessage))]
        //[ServiceKnownType(typeof(AnalogModbusData))]
        //[ServiceKnownType(typeof(DigitalModbusData))]
        void Publish(IPublication publication);
    }
}
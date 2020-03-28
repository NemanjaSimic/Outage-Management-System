using Microsoft.ServiceFabric.Services.Remoting;
using Outage.Common.Exceptions.SCADA;
using Outage.Common.PubSub.SCADADataContract;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Outage.Common.ServiceContracts.SCADA
{
    [ServiceContract]
    public interface IScadaIntegrityUpdateContract : IService
    {
        [OperationContract]
        [FaultContract(typeof(ArgumentException))]
        [FaultContract(typeof(InternalSCADAServiceException))]
        [ServiceKnownType(typeof(SCADAPublication))]
        [ServiceKnownType(typeof(SingleAnalogValueSCADAMessage))]
        [ServiceKnownType(typeof(MultipleAnalogValueSCADAMessage))]
        [ServiceKnownType(typeof(SingleDiscreteValueSCADAMessage))]
        [ServiceKnownType(typeof(MultipleDiscreteValueSCADAMessage))]
        Dictionary<Topic, SCADAPublication> GetIntegrityUpdate();

        [OperationContract]
        [FaultContract(typeof(ArgumentException))]
        [FaultContract(typeof(InternalSCADAServiceException))]
        [ServiceKnownType(typeof(SCADAPublication))]
        [ServiceKnownType(typeof(SingleAnalogValueSCADAMessage))]
        [ServiceKnownType(typeof(MultipleAnalogValueSCADAMessage))]
        [ServiceKnownType(typeof(SingleDiscreteValueSCADAMessage))]
        [ServiceKnownType(typeof(MultipleDiscreteValueSCADAMessage))]
        SCADAPublication GetIntegrityUpdateForSpecificTopic(Topic topic);
    }
}

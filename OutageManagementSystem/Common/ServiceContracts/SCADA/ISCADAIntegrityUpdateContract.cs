using Outage.Common.Exceptions.SCADA;
using Outage.Common.PubSub.SCADADataContract;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Outage.Common.ServiceContracts.SCADA
{
    [ServiceContract]
    public interface ISCADAIntegrityUpdateContract
    {

        [OperationContract]
        [FaultContract(typeof(ArgumentException))]
        [FaultContract(typeof(InternalSCADAServiceException))]
        Dictionary<Topic, SCADAPublication> GetIntegrityUpdate();

        [OperationContract]
        [FaultContract(typeof(ArgumentException))]
        [FaultContract(typeof(InternalSCADAServiceException))]
        SCADAPublication GetIntegrityUpdateForSpecificTopic(Topic topic);
    }
}

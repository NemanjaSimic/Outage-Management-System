using Microsoft.ServiceFabric.Services.Remoting;
using Outage.Common.Exceptions.SCADA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.ServiceContracts.SCADA
{
    [ServiceContract]
    public interface IScadaCommandingContract : IService
    {
        [OperationContract]
        [FaultContract(typeof(ArgumentException))]
        [FaultContract(typeof(InternalSCADAServiceException))]
        bool SendAnalogCommand(long gid, float commandingValue, CommandOriginType commandOriginType);

        [OperationContract]
        [FaultContract(typeof(ArgumentException))]
        [FaultContract(typeof(InternalSCADAServiceException))]
        bool SendDiscreteCommand(long gid, ushort commandingValue, CommandOriginType commandOriginType);
    }
}

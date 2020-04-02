using Microsoft.ServiceFabric.Services.Remoting;
using Outage.Common;
using Outage.Common.Exceptions.SCADA;
using System;
using System.ServiceModel;

namespace OMS.Common.ScadaContracts
{
    [ServiceContract]
    public interface IScadaCommandingContract : IService
    {
        [OperationContract]
        [FaultContract(typeof(ArgumentException))]
        [FaultContract(typeof(InternalSCADAServiceException))]
        Task SendAnalogCommand(long gid, float commandingValue, CommandOriginType commandOriginType);

        [OperationContract]
        [FaultContract(typeof(ArgumentException))]
        [FaultContract(typeof(InternalSCADAServiceException))]
        Task SendDiscreteCommand(long gid, ushort commandingValue, CommandOriginType commandOriginType);
    }
}

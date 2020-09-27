using Common.CloudContracts;
using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Exceptions.SCADA;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace OMS.Common.ScadaContracts.Commanding
{
    [ServiceContract]
    public interface IScadaCommandingContract : IService, IHealthChecker
    {
        [OperationContract]
        [FaultContract(typeof(ArgumentException))]
        [FaultContract(typeof(InternalSCADAServiceException))]
        Task<bool> SendSingleAnalogCommand(long gid, float commandingValue, CommandOriginType commandOriginType);

        [OperationContract]
        [FaultContract(typeof(ArgumentException))]
        [FaultContract(typeof(InternalSCADAServiceException))]
        Task<bool> SendMultipleAnalogCommand(Dictionary<long, float> commandingValues, CommandOriginType commandOriginType);

        [OperationContract]
        [FaultContract(typeof(ArgumentException))]
        [FaultContract(typeof(InternalSCADAServiceException))]
        Task<bool> SendSingleDiscreteCommand(long gid, ushort commandingValue, CommandOriginType commandOriginType);

        [OperationContract]
        [FaultContract(typeof(ArgumentException))]
        [FaultContract(typeof(InternalSCADAServiceException))]
        Task<bool> SendMultipleDiscreteCommand(Dictionary<long, ushort> commandingValues, CommandOriginType commandOriginType);
    }
}

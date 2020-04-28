using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.SCADA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Common.ScadaContracts
{
    [ServiceContract]
    public interface IWriteCommandEnqueuer : IService
    {
        [OperationContract]
        Task<bool> EnqueueWriteCommand(IWriteModbusFunction modbusFunction);
    }
}

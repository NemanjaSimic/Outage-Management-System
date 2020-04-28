using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.SCADA;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace OMS.Common.ScadaContracts
{
    [ServiceContract]
    public interface IModelUpdateCommandEnqueuer : IService
    {
        [OperationContract]
        Task<bool> EnqueueModelUpdateCommands(List<IWriteModbusFunction> modbusFunctions);
    }
}

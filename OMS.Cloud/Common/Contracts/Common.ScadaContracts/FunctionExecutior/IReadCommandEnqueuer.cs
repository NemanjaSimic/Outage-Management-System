using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.SCADA;
using System.ServiceModel;
using System.Threading.Tasks;

namespace OMS.Common.ScadaContracts
{
    [ServiceContract]
    public interface IReadCommandEnqueuer : IService
    {
        [OperationContract]
        Task<bool> EnqueueReadCommand(IReadModbusFunction modbusFunction);
    }
}

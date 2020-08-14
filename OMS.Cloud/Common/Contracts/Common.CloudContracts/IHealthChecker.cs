using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.CloudContracts
{
    [ServiceContract]
    public interface IHealthChecker : IService
    {
        [OperationContract]
        Task<bool> IsAlive();
    }
}

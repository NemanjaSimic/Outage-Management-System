using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;
using System.Threading.Tasks;

namespace OMS.Common.DistributedTransactionContracts
{
    [ServiceContract]
    public interface ITransactionCoordinatorContract : IService
    {
        [OperationContract]
        Task StartDistributedUpdate();

        [OperationContract]
        Task FinishDistributedUpdate(bool success);
    }
}

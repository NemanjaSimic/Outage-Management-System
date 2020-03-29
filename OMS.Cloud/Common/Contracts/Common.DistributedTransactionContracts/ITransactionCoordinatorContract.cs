using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace OMS.Common.DistributedTransactionContracts
{
    [ServiceContract]
    public interface ITransactionCoordinatorContract : IService
    {
        [OperationContract]
        void StartDistributedUpdate();

        [OperationContract]
        void FinishDistributedUpdate(bool success);
    }
}

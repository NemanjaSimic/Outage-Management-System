using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace Outage.Common.ServiceContracts.DistributedTransaction
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

using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace Outage.Common.ServiceContracts.DistributedTransaction
{
    [ServiceContract]
    public interface ITransactionActorContract : IService
    {
        [OperationContract]
        bool Prepare();
        
        [OperationContract]
        void Commit();
        
        [OperationContract]
        void Rollback();
    }
}

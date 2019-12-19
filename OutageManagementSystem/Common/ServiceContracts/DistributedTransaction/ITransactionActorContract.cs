using System.ServiceModel;

namespace Outage.Common.ServiceContracts.DistributedTransaction
{
    [ServiceContract]
    public interface ITransactionActorContract
    {
        [OperationContract]
        bool Prepare();
        
        [OperationContract]
        void Commit();
        
        [OperationContract]
        void Rollback();
    }
}

using System.ServiceModel;

namespace Outage.Common.ServiceContracts.DistributedTransaction
{
    [ServiceContract]
    public interface ITransactionCoordinatorContract
    {
        [OperationContract]
        void StartDistributedUpdate();

        [OperationContract]
        void FinishDistributedUpdate(bool success);
    }

    [ServiceContract]
    public interface ITransactionEnlistmentContract
    {
        [OperationContract]
        bool Enlist(string actorName);
    }
}

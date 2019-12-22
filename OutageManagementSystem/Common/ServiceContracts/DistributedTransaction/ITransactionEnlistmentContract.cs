using System.ServiceModel;

namespace Outage.Common.ServiceContracts.DistributedTransaction
{
    [ServiceContract]
    public interface ITransactionEnlistmentContract
    {
        [OperationContract]
        bool Enlist(string actorName);
    }
}

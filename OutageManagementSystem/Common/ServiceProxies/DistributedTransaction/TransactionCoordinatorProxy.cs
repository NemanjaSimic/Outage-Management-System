using Outage.Common.ServiceContracts.DistributedTransaction;
using System.ServiceModel;

namespace Outage.Common.ServiceProxies.DistributedTransaction
{
    public class TransactionCoordinatorProxy : ClientBase<ITransactionCoordinatorContract>, ITransactionCoordinatorContract
    {
        public TransactionCoordinatorProxy(string endpointName)
            : base(endpointName)
        {
        }

        public void StartDistributedUpdate()
        {
            Channel.StartDistributedUpdate();
        }

        public void FinishDistributedUpdate(bool success)
        {
            Channel.FinishDistributedUpdate(success);
        }
    }

    public class TransactionEnlistmentProxy : ClientBase<ITransactionEnlistmentContract>, ITransactionEnlistmentContract
    { 
        public TransactionEnlistmentProxy(string endpointName)
            : base(endpointName)
        { 
        }

        public bool Enlist(string actorName)
        {
            return Channel.Enlist(actorName);
        }
    }
}

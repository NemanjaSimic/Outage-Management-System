using Outage.Common.ServiceContracts.DistributedTransaction;

namespace Outage.DistributedTransactionActor
{
    public abstract class TransactionActor : ITransactionActorContract
    {
        public virtual bool Prepare()
        {
            return true;
        }

        public abstract void Commit();

        public abstract void Rollback();
    }
}

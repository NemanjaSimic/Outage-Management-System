using Outage.Common.GDA;
using Outage.Common.ServiceContracts.DistributedTransaction;
using System.Collections.Generic;

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

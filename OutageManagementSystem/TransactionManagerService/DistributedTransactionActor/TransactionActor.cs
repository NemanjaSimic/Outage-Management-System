using Outage.Common;
using Outage.Common.ServiceContracts.DistributedTransaction;

namespace Outage.DistributedTransactionActor
{
    public abstract class TransactionActor : ITransactionActorContract
    {
        protected ILogger logger = LoggerWrapper.Instance;

        public virtual bool Prepare()
        {
            logger.LogInfo("Prepare finished SUCCESSFULLY");
            return true;
        }

        public abstract void Commit();

        public abstract void Rollback();
    }
}

using Outage.Common;
using Outage.Common.ServiceContracts.DistributedTransaction;

namespace Outage.DistributedTransactionActor
{
    public abstract class TransactionActor : ITransactionActorContract
    {
        protected ILogger Logger = LoggerWrapper.Instance;

        public virtual bool Prepare()
        {
            Logger.LogInfo("Prepare finished SUCCESSFULLY");
            return true;
        }

        public abstract void Commit();

        public abstract void Rollback();
    }
}

using Outage.Common;
using Outage.Common.ServiceContracts.DistributedTransaction;
using System.Threading.Tasks;

namespace Outage.DistributedTransactionActor
{
    public abstract class TransactionActor : ITransactionActorContract
    {
        protected ILogger Logger = LoggerWrapper.Instance;

        public virtual async Task<bool> Prepare()
        {
            Logger.LogInfo("Prepare finished SUCCESSFULLY");
            return true;
        }

        public abstract Task Commit();

        public abstract Task Rollback();
    }
}

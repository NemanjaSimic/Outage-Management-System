using Outage.Common.ServiceContracts.DistributedTransaction;
using System.ServiceModel;

namespace Outage.Common.ServiceProxies.DistributedTransaction
{
    public class TransactionActorProxy : ClientBase<ITransactionActorContract>, ITransactionActorContract
    {
        public TransactionActorProxy(string endpointName)
            : base(endpointName)
        {
        }

        public bool Prepare()
        {
            return Channel.Prepare();
        }

        public void Commit()
        {
            Channel.Commit();
        }

        public void Rollback()
        {
            Channel.Rollback();
        }
    }
}

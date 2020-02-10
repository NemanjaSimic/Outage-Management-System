using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.ServiceContracts.DistributedTransaction;
using Outage.Common.ServiceProxies;
using System.Collections.Generic;

namespace Outage.DistributedTransactionActor
{
    public abstract class ModelUpdateNotification : IModelUpdateNotificationContract
    {
        protected ILogger Logger = LoggerWrapper.Instance;
        protected ProxyFactory proxyFactory;

        public string TransactionEnlistmentEndpointName { get; private set; }

        public string ActorName { get; set; }

        protected ModelUpdateNotification(string transactionEnlistmentEndpointName, string actorName)
        {
            proxyFactory = new ProxyFactory();
            TransactionEnlistmentEndpointName = transactionEnlistmentEndpointName;
            ActorName = actorName;
        }

        public abstract bool NotifyAboutUpdate(Dictionary<DeltaOpType, List<long>> modelChanges);
    }
}
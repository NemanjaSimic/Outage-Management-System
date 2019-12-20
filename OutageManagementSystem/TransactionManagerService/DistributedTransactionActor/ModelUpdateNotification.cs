using Outage.Common.GDA;
using Outage.Common.ServiceContracts.DistributedTransaction;
using Outage.Common.ServiceProxies.DistributedTransaction;
using System;
using System.Collections.Generic;

namespace Outage.DistributedTransactionActor
{
    public abstract class ModelUpdateNotification : IModelUpdateNotificationContract
    {
        private TransactionEnlistmentProxy transactionEnlistmentProxy = null;

        public string EndpointName { get; private set; }

        public string ActorName { get; set; }

        public TransactionEnlistmentProxy TransactionEnlistmentProxy
        {
            get
            {
                if (transactionEnlistmentProxy != null)
                {
                    transactionEnlistmentProxy.Abort();
                    transactionEnlistmentProxy = null;
                }

                transactionEnlistmentProxy = new TransactionEnlistmentProxy(EndpointName);
                transactionEnlistmentProxy.Open();

                return transactionEnlistmentProxy;
            }
        }

        public ModelUpdateNotification(string endpointName, string actorName)
        {
            EndpointName = endpointName;
            ActorName = actorName;
        }

        public abstract bool NotifyAboutUpdate(Dictionary<DeltaOpType, List<long>> modelChanges);
    }
}
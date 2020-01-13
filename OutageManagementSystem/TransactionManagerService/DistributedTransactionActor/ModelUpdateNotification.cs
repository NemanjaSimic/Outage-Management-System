using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.ServiceContracts.DistributedTransaction;
using Outage.Common.ServiceProxies.DistributedTransaction;
using System;
using System.Collections.Generic;

namespace Outage.DistributedTransactionActor
{
    public abstract class ModelUpdateNotification : IModelUpdateNotificationContract
    {
        protected ILogger Logger = LoggerWrapper.Instance; 

        public string TransactionEnlistmentEndpointName { get; private set; }

        public string ActorName { get; set; }

        #region Proxies
        private TransactionEnlistmentProxy transactionEnlistmentProxy = null;
        protected TransactionEnlistmentProxy TransactionEnlistmentProxy
        {
            get
            {
                try
                {
                    if (transactionEnlistmentProxy != null)
                    {
                        transactionEnlistmentProxy.Abort();
                        transactionEnlistmentProxy = null;
                    }

                    transactionEnlistmentProxy = new TransactionEnlistmentProxy(TransactionEnlistmentEndpointName);
                    transactionEnlistmentProxy.Open();
                }
                catch (Exception ex)
                {
                    string message = $"Exception on TransactionEnlistmentProxy initialization. Message: {ex.Message}";
                    Logger.LogError(message, ex);
                    transactionEnlistmentProxy = null;
                }

                return transactionEnlistmentProxy;
            }
        }
        #endregion

        protected ModelUpdateNotification(string transactionEnlistmentEndpointName, string actorName)
        {
            TransactionEnlistmentEndpointName = transactionEnlistmentEndpointName;
            ActorName = actorName;
        }

        public abstract bool NotifyAboutUpdate(Dictionary<DeltaOpType, List<long>> modelChanges);
    }
}
﻿using Outage.Common;
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
        protected ILogger logger = LoggerWrapper.Instance; 

        public string TransactionEnlistmentEndpointName { get; private set; }

        public string ActorName { get; set; }

        protected TransactionEnlistmentProxy TransactionEnlistmentProxy
        {
            get
            {
                if (transactionEnlistmentProxy != null)
                {
                    transactionEnlistmentProxy.Abort();
                    transactionEnlistmentProxy = null;
                }

                transactionEnlistmentProxy = new TransactionEnlistmentProxy(TransactionEnlistmentEndpointName);
                transactionEnlistmentProxy.Open();

                return transactionEnlistmentProxy;
            }
        }

        protected ModelUpdateNotification(string transactionEnlistmentEndpointName, string actorName)
        {
            TransactionEnlistmentEndpointName = transactionEnlistmentEndpointName;
            ActorName = actorName;
        }

        public abstract bool NotifyAboutUpdate(Dictionary<DeltaOpType, List<long>> modelChanges);
    }
}
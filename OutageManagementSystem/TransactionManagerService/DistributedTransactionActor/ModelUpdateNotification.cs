using Outage.Common.GDA;
using Outage.Common.ServiceContracts.DistributedTransaction;
using Outage.Common.ServiceProxies.DistributedTransaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.DistributedTransactionActor
{
    public abstract class ModelUpdateNotification : IModelUpdateNotificationContract
    { 
        private TransactionCoordinatorEnlistmentProxy transactionEnlistmentProxy = null;

        public string EndpointName { get; private set; }

        public string ActorName { get; set; }

        public TransactionCoordinatorEnlistmentProxy TransactionEnlistmentProxy
        {
            get
            {
                if (transactionEnlistmentProxy != null)
                {
                    transactionEnlistmentProxy.Abort();
                    transactionEnlistmentProxy = null;
                }

                transactionEnlistmentProxy = new TransactionCoordinatorEnlistmentProxy(EndpointName);
                transactionEnlistmentProxy.Open();

                return transactionEnlistmentProxy;
            }
        }

        public ModelUpdateNotification(string endpointName, string actorName)
        {
            EndpointName = endpointName;
            ActorName = actorName;
        }

        public virtual bool NotifyAboutUpdate(Dictionary<DeltaOpType, List<long>> modelChanges)
        {
            bool success = false;

            try
            {
                using (TransactionEnlistmentProxy)
                {
                    success = TransactionEnlistmentProxy.Enlist(ActorName);
                }
            }
            catch (Exception e)
            {

                success = false;
            }

            return success;
        }
    }
}

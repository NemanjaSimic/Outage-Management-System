using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.ServiceContracts.DistributedTransaction;
using Outage.Common.ServiceProxies.DistributedTransaction;
using Outage.DistributedTransactionActor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutageManagementService.DistribuedTransaction
{
    public class OutageModelUpdateNotification : ModelUpdateNotification
    {

        #region Static Members
        protected static OutageModel outageModel = null;

        public static OutageModel OutageModel
        {
            set
            {
                if (outageModel == null)
                {
                    outageModel = value;
                }
            }
        }
        #endregion

        public OutageModelUpdateNotification()
            : base(EndpointNames.TransactionEnlistmentEndpoint, ServiceNames.OutageManagementService)
        {

        }


        public override bool NotifyAboutUpdate(Dictionary<DeltaOpType, List<long>> modelChanges)
        {
            bool success = OutageModelUpdateNotification.outageModel.Notify(modelChanges);

            if (success)
            {
                using (TransactionEnlistmentProxy transactionEnlistmentProxy = proxyFactory.CreateProxy<TransactionEnlistmentProxy, ITransactionEnlistmentContract>(EndpointNames.TransactionEnlistmentEndpoint))
                {
                    if (transactionEnlistmentProxy != null)
                    {
                        transactionEnlistmentProxy.Enlist(ActorName);
                    }
                    else
                    {
                        string message = "TransactionEnlistmentProxy is null";
                        Logger.LogWarn(message);
                        throw new NullReferenceException(message);
                    }
                }

                Logger.LogInfo("Outage SUCCESSFULLY notified about network model update.");
            }
            else
            {
                Logger.LogInfo("Outage UNSUCCESSFULLY notified about network model update.");
            }

            return success;
        }
    }
}

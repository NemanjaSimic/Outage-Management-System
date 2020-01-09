using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.ServiceProxies.DistributedTransaction;
using Outage.DistributedTransactionActor;
using System;
using System.Collections.Generic;

namespace SCADA_Service.DistributedTransaction
{
    [Obsolete]
    public class SCADAModelUpdateNotification : ModelUpdateNotification
    {
        public SCADAModelUpdateNotification()
            : base(EndpointNames.TransactionEnlistmentEndpoint, ServiceNames.SCADAService)
        {
        }

        public override bool NotifyAboutUpdate(Dictionary<DeltaOpType, List<long>> modelChanges)
        {
            //TODO: SCADA notification logic

            using (TransactionEnlistmentProxy transactionEnlistmentProxy = TransactionEnlistmentProxy)
            {
                if (transactionEnlistmentProxy != null)
                {
                    transactionEnlistmentProxy.Enlist(ActorName);
                }
                else
                {
                    string message = "TransactionEnlistmentProxy is null.";
                    logger.LogWarn(message);
                    throw new NullReferenceException(message);
                }
            }

            logger.LogInfo("SCADA SUCCESSFULLY notified about network model update.");
            return true;
        }
    }
}
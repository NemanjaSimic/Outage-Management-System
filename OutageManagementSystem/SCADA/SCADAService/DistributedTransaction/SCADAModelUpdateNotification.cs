using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.ServiceProxies.DistributedTransaction;
using Outage.DistributedTransactionActor;
using System;
using System.Collections.Generic;

namespace Outage.SCADA.SCADAService.DistributedTransaction
{
    public class SCADAModelUpdateNotification : ModelUpdateNotification
    {
        protected static SCADAModel scadaModel = null;

        public static SCADAModel SCADAModel
        {
            set
            {
                scadaModel = value;
            }
        }

        public SCADAModelUpdateNotification()
            : base(EndpointNames.TransactionEnlistmentEndpoint, ServiceNames.SCADAService)
        {
        }

        public override bool NotifyAboutUpdate(Dictionary<DeltaOpType, List<long>> modelChanges)
        {
            bool success = scadaModel.Notify(modelChanges);

            if (success)
            {
                using (TransactionEnlistmentProxy transactionEnlistmentProxy = TransactionEnlistmentProxy)
                {
                    if (transactionEnlistmentProxy != null)
                    {
                        transactionEnlistmentProxy.Enlist(ActorName);
                    }
                    else
                    {
                        string message = "TransactionEnlistmentProxy is null";
                        logger.LogWarn(message);
                        throw new NullReferenceException(message);
                    }
                }

                logger.LogInfo("SCADA SUCCESSFULLY notified about network model update.");
            }
            else
            {
                logger.LogInfo("SCADA UNSUCCESSFULLY notified about network model update.");
            }

            return success;
        }
    }
}
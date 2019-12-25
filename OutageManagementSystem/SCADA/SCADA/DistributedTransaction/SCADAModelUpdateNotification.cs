using Outage.Common;
using Outage.Common.GDA;
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

            TransactionEnlistmentProxy.Enlist(ActorName);
            logger.LogInfo("SCADA SUCCESSFULLY notified about network model update.");
            return true;
        }
    }
}
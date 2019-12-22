using Outage.Common;
using Outage.Common.GDA;
using Outage.DistributedTransactionActor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA_Service.DistributedTransaction
{
    public class SCADAModelUpdateNotification : ModelUpdateNotification
    {
        public SCADAModelUpdateNotification()
            : base(EndpointNames.SCADAModelUpdateNotifierEndpoint, ServiceNames.SCADAService)
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

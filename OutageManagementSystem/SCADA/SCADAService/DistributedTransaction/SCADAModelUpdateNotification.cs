using Outage.Common;
using Outage.Common.GDA;
using Outage.DistributedTransactionActor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            if(success)
            {
                TransactionEnlistmentProxy.Enlist(ActorName);
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

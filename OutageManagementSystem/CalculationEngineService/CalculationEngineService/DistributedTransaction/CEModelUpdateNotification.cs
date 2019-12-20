using Outage.Common;
using Outage.Common.GDA;
using Outage.DistributedTransactionActor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculationEngineService.DistributedTransaction
{
    public class CEModelUpdateNotification : ModelUpdateNotification
    {
        public CEModelUpdateNotification()
            : base(EndpointNames.CalculationEngineModelUpdateNotifierEndpoint, ServiceNames.CalculationEngineService)
        {
        }

        public override bool NotifyAboutUpdate(Dictionary<DeltaOpType, List<long>> modelChanges)
        {
            throw new NotImplementedException();
        }
    }
}

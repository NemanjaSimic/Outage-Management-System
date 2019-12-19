using Outage.Common.GDA;
using Outage.Common.ServiceContracts.DistributedTransaction;
using System.Collections.Generic;
using System.ServiceModel;

namespace Outage.Common.ServiceProxies.DistributedTransaction
{
    public class ModelUpdateNotificationProxy : ClientBase<IModelUpdateNotificationContract>, IModelUpdateNotificationContract
    {
        public ModelUpdateNotificationProxy(string endpointName)
            : base(endpointName)
        {
        }

        public bool NotifyAboutUpdate(Dictionary<DeltaOpType, List<long>> modelChanges)
        {
            return Channel.NotifyAboutUpdate(modelChanges);
        }
    }
}

using Outage.Common.GDA;
using Outage.Common.ServiceContracts.DistributedTransaction;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Outage.Common.ServiceProxies.DistributedTransaction
{
    public class ModelUpdateNotificationProxy : BaseProxy<IModelUpdateNotificationContract>, IModelUpdateNotificationContract
    {
        public ModelUpdateNotificationProxy(string endpointName)
            : base(endpointName)
        {
        }

        public bool NotifyAboutUpdate(Dictionary<DeltaOpType, List<long>> modelChanges)
        {
            bool success;

            try
            {
                success = Channel.NotifyAboutUpdate(modelChanges);
            }
            catch (Exception e)
            {
                string message = "Exception in NotifyAboutUpdate() proxy method.";
                LoggerWrapper.Instance.LogError(message, e);
                throw e;
            }

            return success;
        }
    }
}

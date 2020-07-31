using Microsoft.ServiceFabric.Services.Remoting;
using Outage.Common.GDA;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace Outage.Common.ServiceContracts.DistributedTransaction
{
    [ServiceContract]
    [Obsolete("Use OMS.Common.DistributedTransactionContracts")]
    public interface IModelUpdateNotificationContract
    {
        [OperationContract]
        [Obsolete("Use OMS.Common.DistributedTransactionContracts")]
        bool NotifyAboutUpdate(Dictionary<DeltaOpType, List<long>> modelChanges);
    }
}

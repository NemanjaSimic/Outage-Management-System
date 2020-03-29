using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.NmsContracts.GDA;
using System.Collections.Generic;
using System.ServiceModel;

namespace OMS.Common.DistributedTransactionContracts
{
    [ServiceContract]
    public interface IModelUpdateNotificationContract : IService
    {
        [OperationContract]
        bool NotifyAboutUpdate(Dictionary<DeltaOpType, List<long>> modelChanges);
    }
}

using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.NmsContracts.GDA;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace OMS.Common.DistributedTransactionContracts
{
    [ServiceContract]
    public interface IModelUpdateNotificationContract : IService
    {
        [OperationContract]
        Task<bool> NotifyAboutUpdate(Dictionary<DeltaOpType, List<long>> modelChanges);
    }
}

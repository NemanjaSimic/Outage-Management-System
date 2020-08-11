using Common.CloudContracts;
using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.NmsContracts.GDA;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace OMS.Common.TmsContracts.Notifications
{
    [ServiceContract]
    public interface INotifyNetworkModelUpdateContract : IService, IHealthChecker
    {
        [OperationContract]
        Task<bool> Notify(Dictionary<DeltaOpType, List<long>> modelChanges);
    }
}

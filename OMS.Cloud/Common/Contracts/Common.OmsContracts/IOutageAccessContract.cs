using Common.CloudContracts;
using Common.PubSubContracts.DataContracts.OMS;
using Microsoft.ServiceFabric.Services.Remoting;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.OmsContracts
{
    [ServiceContract]
    public interface IOutageAccessContract : IService, IHealthChecker
    {
        [OperationContract]
        Task<IEnumerable<ActiveOutageMessage>> GetActiveOutages();

        [OperationContract]
        Task<IEnumerable<ArchivedOutageMessage>> GetArchivedOutages();
    }
}

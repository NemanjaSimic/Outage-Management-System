using Common.CloudContracts;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;
using System.Threading.Tasks;

namespace OMS.Common.TmsContracts
{
    [ServiceContract]
    public interface ITransactionActorContract : IService, IHealthChecker
    {
        [OperationContract]
        Task<bool> Prepare();
        
        [OperationContract]
        Task Commit();
        
        [OperationContract]
        Task Rollback();
    }
}

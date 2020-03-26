using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Outage.Common.ServiceContracts.DistributedTransaction
{
    [ServiceContract]
    public interface ITransactionActorContract : IService
    {
        [OperationContract]
        Task<bool> Prepare();
        
        [OperationContract]
        Task Commit();
        
        [OperationContract]
        Task Rollback();
    }
}

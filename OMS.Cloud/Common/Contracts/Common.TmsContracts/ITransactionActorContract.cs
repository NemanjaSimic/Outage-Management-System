using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;
using System.Threading.Tasks;

namespace OMS.Common.TmsContracts
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

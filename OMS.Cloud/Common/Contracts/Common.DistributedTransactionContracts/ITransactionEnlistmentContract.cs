using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;
using System.Threading.Tasks;

namespace OMS.Common.DistributedTransactionContracts
{
    [ServiceContract]
    public interface ITransactionEnlistmentContract : IService
    {
        [OperationContract]
        Task<bool> Enlist(string actorName);
    }
}

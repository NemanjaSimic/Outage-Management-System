using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace Outage.Common.ServiceContracts.DistributedTransaction
{
    [ServiceContract]
    public interface ITransactionEnlistmentContract : IService
    {
        [OperationContract]
        bool Enlist(string actorName);
    }
}

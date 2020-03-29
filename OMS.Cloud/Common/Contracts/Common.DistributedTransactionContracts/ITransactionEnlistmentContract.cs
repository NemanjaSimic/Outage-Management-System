using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace OMS.Common.DistributedTransactionContracts
{
    [ServiceContract]
    public interface ITransactionEnlistmentContract : IService
    {
        [OperationContract]
        bool Enlist(string actorName);
    }
}

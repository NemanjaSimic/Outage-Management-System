using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.ServiceModel;

namespace Outage.Common.ServiceContracts.DistributedTransaction
{
    [ServiceContract]
    [Obsolete("Use OMS.Common.DistributedTransactionContracts")]
    public interface ITransactionEnlistmentContract : IService
    {
        [OperationContract]
        [Obsolete("Use OMS.Common.DistributedTransactionContracts")]
        bool Enlist(string actorName);
    }
}

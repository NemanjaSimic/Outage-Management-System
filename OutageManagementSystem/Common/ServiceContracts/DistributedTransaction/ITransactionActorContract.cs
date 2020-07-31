using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Outage.Common.ServiceContracts.DistributedTransaction
{
    [ServiceContract]
    [Obsolete("Use OMS.Common.DistributedTransactionContracts")]
    public interface ITransactionActorContract
    {
        [OperationContract]
        [Obsolete("Use OMS.Common.DistributedTransactionContracts")]
        Task<bool> Prepare();
        
        [OperationContract]
        [Obsolete("Use OMS.Common.DistributedTransactionContracts")]
        Task Commit();
        
        [OperationContract]
        [Obsolete("Use OMS.Common.DistributedTransactionContracts")]
        Task Rollback();
    }
}

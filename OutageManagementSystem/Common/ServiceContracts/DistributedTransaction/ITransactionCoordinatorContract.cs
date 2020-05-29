using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.ServiceModel;

namespace Outage.Common.ServiceContracts.DistributedTransaction
{
    [ServiceContract]
    [Obsolete("Use OMS.Common.DistributedTransactionContracts")]
    public interface ITransactionCoordinatorContract : IService
    {
        [OperationContract]
        [Obsolete("Use OMS.Common.DistributedTransactionContracts")]
        void StartDistributedUpdate();

        [OperationContract]
        [Obsolete("Use OMS.Common.DistributedTransactionContracts")]
        void FinishDistributedUpdate(bool success);
    }
}

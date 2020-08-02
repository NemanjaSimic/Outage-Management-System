using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace OMS.Common.TmsContracts
{
    [ServiceContract]
    public interface ITransactionCoordinatorContract : IService
    {
        [OperationContract]
        Task StartDistributedTransaction(string transactionName, IEnumerable<string> transactionActors);

        [OperationContract]
        Task FinishDistributedTransaction(string transactionName, bool success);
    }
}

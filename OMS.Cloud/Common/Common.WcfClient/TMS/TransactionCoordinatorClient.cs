using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Names;
using OMS.Common.TmsContracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.TMS
{
    public class TransactionCoordinatorClient : WcfSeviceFabricClientBase<ITransactionCoordinatorContract>, ITransactionCoordinatorContract
    {
        private static readonly string microserviceName = MicroserviceNames.TransactionManagerService;
        private static readonly string listenerName = EndpointNames.TmsTransactionCoordinatorEndpoint;

        public TransactionCoordinatorClient(WcfCommunicationClientFactory<ITransactionCoordinatorContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
            : base(clientFactory, serviceUri, servicePartition, listenerName)
        {
        }

        public static ITransactionCoordinatorContract CreateClient()
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<TransactionCoordinatorClient, ITransactionCoordinatorContract>(microserviceName);
        }

        public static ITransactionCoordinatorContract CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<TransactionCoordinatorClient, ITransactionCoordinatorContract>(serviceUri, servicePartitionKey);
        }

        #region ITransactionCoordinatorContract
        public Task StartDistributedTransaction(string transactionName, IEnumerable<string> transactionActors)
        {
            //return MethodWrapperAsync("StartDistributedTransaction", new object[2] { transactionName, transactionActors });
            return InvokeWithRetryAsync(client => client.Channel.StartDistributedTransaction(transactionName, transactionActors));
        }

        public Task FinishDistributedTransaction(string transactionName, bool success)
        {
            //return MethodWrapperAsync("FinishDistributedTransaction", new object[2] { transactionName, success });
            return InvokeWithRetryAsync(client => client.Channel.FinishDistributedTransaction(transactionName, success));
        }
        #endregion

        public Task<bool> IsAlive()
        {
            return InvokeWithRetryAsync(client => client.Channel.IsAlive());
        }
    }
}

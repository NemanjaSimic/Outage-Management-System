using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using OMS.Common.TmsContracts;
using System;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.TMS
{
    public class TransactionEnlistmentClient : WcfSeviceFabricClientBase<ITransactionEnlistmentContract>, ITransactionEnlistmentContract
    {
        private static readonly string microserviceName = MicroserviceNames.TransactionManagerService;
        private static readonly string listenerName = EndpointNames.TmsTransactionEnlistmentEndpoint;

        public TransactionEnlistmentClient(WcfCommunicationClientFactory<ITransactionEnlistmentContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
           : base(clientFactory, serviceUri, servicePartition, listenerName)
        {
        }

        public static ITransactionEnlistmentContract CreateClient()
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<TransactionEnlistmentClient, ITransactionEnlistmentContract>(microserviceName);
        }

        public static ITransactionEnlistmentContract CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<TransactionEnlistmentClient, ITransactionEnlistmentContract>(serviceUri, servicePartitionKey);
        }

        #region ITransactionEnlistmentContract
        public Task<bool> Enlist(string transactionName, string transactionActorName)
        {
            //return MethodWrapperAsync<bool>("Enlist", new object[2] { transactionName, transactionActorName });
            return InvokeWithRetryAsync(client => client.Channel.Enlist(transactionName, transactionActorName));
        }
        #endregion

        public Task<bool> IsAlive()
        {
            return InvokeWithRetryAsync(client => client.Channel.IsAlive());
        }
    }
}

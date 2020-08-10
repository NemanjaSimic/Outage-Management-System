using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Names;
using OMS.Common.TmsContracts;
using System;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.TMS
{
    public class TransactionActorClient : WcfSeviceFabricClientBase<ITransactionActorContract>, ITransactionActorContract
    {
        private static readonly string listenerName = EndpointNames.TmsTransactionActorEndpoint;

        public TransactionActorClient(WcfCommunicationClientFactory<ITransactionActorContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
            : base(clientFactory, serviceUri, servicePartition, listenerName)
        {
        }

        public static ITransactionActorContract CreateClient(string serviceName)
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<TransactionActorClient, ITransactionActorContract>(serviceName);
        }

        public static ITransactionActorContract CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<TransactionActorClient, ITransactionActorContract>(serviceUri, servicePartitionKey);
        }

        #region ITransactionActorContract
        public Task<bool> Prepare()
        {
            //return MethodWrapperAsync<bool>("Prepare", new object[0]);
            return InvokeWithRetryAsync(client => client.Channel.Prepare());
        }

        public Task Commit()
        {
            //return MethodWrapperAsync<bool>("Commit", new object[0]);
            return InvokeWithRetryAsync(client => client.Channel.Commit());
        }

        public Task Rollback()
        {
            //return MethodWrapperAsync<bool>("Rollback", new object[0]);
            return InvokeWithRetryAsync(client => client.Channel.Rollback());
        }
        #endregion
    }
}

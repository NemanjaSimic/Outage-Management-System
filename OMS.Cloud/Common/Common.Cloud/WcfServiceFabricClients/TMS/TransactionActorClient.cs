using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.DistributedTransactionContracts;
using System;
using System.Threading.Tasks;

namespace OMS.Common.Cloud.WcfServiceFabricClients.TMS
{
    public class TransactionActorClient : WcfSeviceFabricClientBase<ITransactionActorContract>, ITransactionActorContract
    {
        public TransactionActorClient(WcfCommunicationClientFactory<ITransactionActorContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
            : base(clientFactory, serviceUri, servicePartition)
        {
        }

        public static TransactionActorClient CreateClient(Uri serviceUri = null)
        {
            ClientFactory factory = new ClientFactory();

            if (serviceUri == null)
            {
                return factory.CreateClient<TransactionActorClient, ITransactionActorContract>(MicroserviceNames.TransactionActorService);
            }
            else
            {
                return factory.CreateClient<TransactionActorClient, ITransactionActorContract>(serviceUri);
            }
        }

        #region ITransactionActorContract
        public Task<bool> Prepare()
        {
            return InvokeWithRetryAsync(client => client.Channel.Prepare());
        }

        public Task Commit()
        {
            return InvokeWithRetryAsync(client => client.Channel.Commit());
        }

        public Task Rollback()
        {
            return InvokeWithRetryAsync(client => client.Channel.Rollback());
        }
        #endregion
    }
}

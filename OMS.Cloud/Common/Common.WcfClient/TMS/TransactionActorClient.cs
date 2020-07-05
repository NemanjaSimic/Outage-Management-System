using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Names;
using OMS.Common.DistributedTransactionContracts;
using System;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.TMS
{
    public class TransactionActorClient : WcfSeviceFabricClientBase<ITransactionActorContract>, ITransactionActorContract
    {
        private static readonly string microserviceName = MicroserviceNames.TransactionActorService;
        private static readonly string listenerName = "";

        public TransactionActorClient(WcfCommunicationClientFactory<ITransactionActorContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
            : base(clientFactory, serviceUri, servicePartition, listenerName)
        {
        }

        public static TransactionActorClient CreateClient(Uri serviceUri = null)
        {
            ClientFactory factory = new ClientFactory();

            if (serviceUri == null)
            {
                return factory.CreateClient<TransactionActorClient, ITransactionActorContract>(microserviceName);
            }
            else
            {
                return factory.CreateClient<TransactionActorClient, ITransactionActorContract>(serviceUri);
            }
        }

        #region ITransactionActorContract
        public Task<bool> Prepare()
        {
            return MethodWrapperAsync<bool>("Prepare", new object[0]);
            //return InvokeWithRetryAsync(client => client.Channel.Prepare());
        }

        public Task Commit()
        {
            return MethodWrapperAsync<bool>("Commit", new object[0]);
            //return InvokeWithRetryAsync(client => client.Channel.Commit());
        }

        public Task Rollback()
        {
            return MethodWrapperAsync<bool>("Rollback", new object[0]);
            //return InvokeWithRetryAsync(client => client.Channel.Rollback());
        }
        #endregion
    }
}

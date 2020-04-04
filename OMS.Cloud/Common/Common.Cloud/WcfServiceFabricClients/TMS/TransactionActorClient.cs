using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.DistributedTransactionContracts;
using System;
using System.Configuration;
using System.Fabric;
using System.Threading.Tasks;

namespace OMS.Common.Cloud.WcfServiceFabricClients.TMS
{
    public class TransactionActorClient : WcfSeviceFabricClientBase<ITransactionActorContract>, ITransactionActorContract
    {
        public TransactionActorClient(WcfCommunicationClientFactory<ITransactionActorContract> clientFactory, Uri serviceUri)
            : base(clientFactory, serviceUri)
        {
        }

        public static TransactionActorClient CreateClient(Uri serviceUri = null)
        {
            if (serviceUri == null && ConfigurationManager.AppSettings[MicroserviceNames.TransactionActorService] is string transactionActorServiceName)
            {
                serviceUri = new Uri(transactionActorServiceName);
            }

            var partitionResolver = new ServicePartitionResolver(() => new FabricClient());
            //var partitionResolver = ServicePartitionResolver.GetDefault();
            var factory = new WcfCommunicationClientFactory<ITransactionActorContract>(TcpBindingHelper.CreateClientBinding(), null, partitionResolver);

            return new TransactionActorClient(factory, serviceUri);
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

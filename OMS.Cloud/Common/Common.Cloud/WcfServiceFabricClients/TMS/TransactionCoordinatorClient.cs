using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.DistributedTransactionContracts;
using System;
using System.Configuration;
using System.Fabric;
using System.Threading.Tasks;

namespace OMS.Common.Cloud.WcfServiceFabricClients.TMS
{
    public class TransactionCoordinatorClient : WcfSeviceFabricClientBase<ITransactionCoordinatorContract>, ITransactionCoordinatorContract
    {
        public TransactionCoordinatorClient(WcfCommunicationClientFactory<ITransactionCoordinatorContract> clientFactory, Uri serviceUri)
            : base(clientFactory, serviceUri)
        {
        }

        public static TransactionCoordinatorClient CreateClient(Uri serviceUri = null)
        {
            ClientFactory factory = new ClientFactory();

            if (serviceUri == null)
            {
                return factory.CreateClient<TransactionCoordinatorClient, ITransactionCoordinatorContract>(MicroserviceNames.TransactionManagerService);
            }
            else
            {
                return factory.CreateClient<TransactionCoordinatorClient, ITransactionCoordinatorContract>(serviceUri);
            }
        }

        #region ITransactionCoordinatorContract
        public Task FinishDistributedUpdate(bool success)
        {
            return InvokeWithRetryAsync(client => client.Channel.FinishDistributedUpdate(success));
        }

        public Task StartDistributedUpdate()
        {
            return InvokeWithRetryAsync(client => client.Channel.StartDistributedUpdate());
        }
        #endregion
    }
}

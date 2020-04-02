using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.DistributedTransactionContracts;
using System;
using System.Configuration;
using System.Fabric;

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
            if (serviceUri == null && ConfigurationManager.AppSettings[MicroserviceNames.TransactionManagerService] is string transactionManagerServiceName)
            {
                serviceUri = new Uri(transactionManagerServiceName);
            }

            var partitionResolver = new ServicePartitionResolver(() => new FabricClient());
            //var partitionResolver = ServicePartitionResolver.GetDefault();
            var factory = new WcfCommunicationClientFactory<ITransactionCoordinatorContract>(TcpBindingHelper.CreateClientBinding(), null, partitionResolver);

            return new TransactionCoordinatorClient(factory, serviceUri);
        }

        #region ITransactionCoordinatorContract
        public void FinishDistributedUpdate(bool success)
        {
            throw new NotImplementedException();
        }

        public void StartDistributedUpdate()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

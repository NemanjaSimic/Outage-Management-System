using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.DistributedTransactionContracts;
using System;
using System.Configuration;
using System.Fabric;

namespace OMS.Common.Cloud.WcfServiceFabricClients.TMS
{
    public class TransactionEnlistmentClient : WcfSeviceFabricClientBase<ITransactionEnlistmentContract>, ITransactionEnlistmentContract
    {
        public TransactionEnlistmentClient(WcfCommunicationClientFactory<ITransactionEnlistmentContract> clientFactory, Uri serviceUri)
           : base(clientFactory, serviceUri)
        {
        }

        public static TransactionEnlistmentClient CreateClient(Uri serviceUri = null)
        {
            if (serviceUri == null && ConfigurationManager.AppSettings[MicroserviceNames.TransactionManagerService] is string transactionManagerServiceName)
            {
                serviceUri = new Uri(transactionManagerServiceName);
            }

            var partitionResolver = new ServicePartitionResolver(() => new FabricClient());
            //var partitionResolver = ServicePartitionResolver.GetDefault();
            var factory = new WcfCommunicationClientFactory<ITransactionEnlistmentContract>(TcpBindingHelper.CreateClientBinding(), null, partitionResolver);

            return new TransactionEnlistmentClient(factory, serviceUri);
        }

        #region ITransactionEnlistmentContract
        public bool Enlist(string actorName)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

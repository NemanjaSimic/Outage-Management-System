using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.DistributedTransactionContracts;
using OMS.Common.NmsContracts.GDA;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Fabric;

namespace OMS.Common.Cloud.WcfServiceFabricClients.TMS
{
    public class ModelUpdateNotificationClient : WcfSeviceFabricClientBase<IModelUpdateNotificationContract>, IModelUpdateNotificationContract
    {
        public ModelUpdateNotificationClient(WcfCommunicationClientFactory<IModelUpdateNotificationContract> clientFactory, Uri serviceUri)
            : base(clientFactory, serviceUri)
        {
        }

        public static ModelUpdateNotificationClient CreateClient(Uri serviceUri = null)
        {
            if (serviceUri == null && ConfigurationManager.AppSettings[MicroserviceNames.TransactionActorService] is string transactionActorServiceName)
            {
                serviceUri = new Uri(transactionActorServiceName);
            }

            var partitionResolver = new ServicePartitionResolver(() => new FabricClient());
            //var partitionResolver = ServicePartitionResolver.GetDefault();
            var factory = new WcfCommunicationClientFactory<IModelUpdateNotificationContract>(TcpBindingHelper.CreateClientBinding(), null, partitionResolver);

            return new ModelUpdateNotificationClient(factory, serviceUri);
        }

        #region IModelUpdateNotificationContract
        public bool NotifyAboutUpdate(Dictionary<DeltaOpType, List<long>> modelChanges)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}

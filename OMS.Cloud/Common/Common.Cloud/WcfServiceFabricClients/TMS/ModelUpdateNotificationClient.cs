using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.DistributedTransactionContracts;
using OMS.Common.NmsContracts.GDA;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.Common.Cloud.WcfServiceFabricClients.TMS
{
    public class ModelUpdateNotificationClient : WcfSeviceFabricClientBase<IModelUpdateNotificationContract>, IModelUpdateNotificationContract
    {
        public ModelUpdateNotificationClient(WcfCommunicationClientFactory<IModelUpdateNotificationContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
            : base(clientFactory, serviceUri, servicePartition)
        {
        }

        public static ModelUpdateNotificationClient CreateClient(Uri serviceUri = null)
        {
            ClientFactory factory = new ClientFactory();

            if (serviceUri == null)
            {
                return factory.CreateClient<ModelUpdateNotificationClient, IModelUpdateNotificationContract>(MicroserviceNames.TransactionActorService);
            }
            else
            {
                return factory.CreateClient<ModelUpdateNotificationClient, IModelUpdateNotificationContract>(serviceUri);
            }
        }

        #region IModelUpdateNotificationContract
        public Task<bool> NotifyAboutUpdate(Dictionary<DeltaOpType, List<long>> modelChanges)
        {
            return InvokeWithRetryAsync(client => client.Channel.NotifyAboutUpdate(modelChanges));
        }
        #endregion
    }
}

using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using OMS.Common.DistributedTransactionContracts;
using OMS.Common.NmsContracts.GDA;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.TMS
{
    public class ModelUpdateNotificationClient : WcfSeviceFabricClientBase<IModelUpdateNotificationContract>, IModelUpdateNotificationContract
    {
        private static readonly string microserviceName = MicroserviceNames.TransactionActorService;
        private static readonly string listenerName = "";

        public ModelUpdateNotificationClient(WcfCommunicationClientFactory<IModelUpdateNotificationContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
            : base(clientFactory, serviceUri, servicePartition, listenerName)
        {
        }

        public static ModelUpdateNotificationClient CreateClient(Uri serviceUri = null)
        {
            ClientFactory factory = new ClientFactory();

            if (serviceUri == null)
            {
                return factory.CreateClient<ModelUpdateNotificationClient, IModelUpdateNotificationContract>(microserviceName);
            }
            else
            {
                return factory.CreateClient<ModelUpdateNotificationClient, IModelUpdateNotificationContract>(serviceUri);
            }
        }

        #region IModelUpdateNotificationContract
        public Task<bool> NotifyAboutUpdate(Dictionary<DeltaOpType, List<long>> modelChanges)
        {
            return MethodWrapperAsync<bool>("NotifyAboutUpdate", new object[1] { modelChanges });
            //return InvokeWithRetryAsync(client => client.Channel.NotifyAboutUpdate(modelChanges));
        }
        #endregion
    }
}

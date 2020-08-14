using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud;
using OMS.Common.NmsContracts.GDA;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OMS.Common.Cloud.Names;
using OMS.Common.TmsContracts.Notifications;
using System.Configuration;

namespace OMS.Common.WcfClient.TMS
{
    public class NotifyNetworkModelUpdateClient : WcfSeviceFabricClientBase<INotifyNetworkModelUpdateContract>, INotifyNetworkModelUpdateContract
    {
        private static readonly string listenerName = EndpointNames.TmsNotifyNetworkModelUpdateEndpoint;

        public NotifyNetworkModelUpdateClient(WcfCommunicationClientFactory<INotifyNetworkModelUpdateContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
            : base(clientFactory, serviceUri, servicePartition, listenerName)
        {
        }

        public static INotifyNetworkModelUpdateContract CreateClient(string serviceName)
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<NotifyNetworkModelUpdateClient, INotifyNetworkModelUpdateContract>(serviceName);
        }

        public static INotifyNetworkModelUpdateContract CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<NotifyNetworkModelUpdateClient, INotifyNetworkModelUpdateContract>(serviceUri, servicePartitionKey);
        }

        #region IModelUpdateNotificationContract
        public Task<bool> Notify(Dictionary<DeltaOpType, List<long>> modelChanges)
        {
            //return MethodWrapperAsync<bool>("Notify", new object[1] { modelChanges });
            return InvokeWithRetryAsync(client => client.Channel.Notify(modelChanges));
        }
        #endregion
    }
}

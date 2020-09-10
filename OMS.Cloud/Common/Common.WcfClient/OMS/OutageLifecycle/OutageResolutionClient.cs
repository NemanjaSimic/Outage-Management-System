using Common.OmsContracts.OutageLifecycle;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using System;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.OMS.OutageLifecycle
{
    public class OutageResolutionClient : WcfSeviceFabricClientBase<IOutageResolutionContract>, IOutageResolutionContract
    {
        private static readonly string microserviceName = MicroserviceNames.OmsOutageLifecycleService;
        private static readonly string listenerName = EndpointNames.OmsOutageResolutionEndpoint;
        
        public OutageResolutionClient(WcfCommunicationClientFactory<IOutageResolutionContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
           : base(clientFactory, serviceUri, servicePartition, listenerName)
        {
        }

        public static IOutageResolutionContract CreateClient()
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<OutageResolutionClient, IOutageResolutionContract>(microserviceName);
        }

        public static IOutageResolutionContract CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<OutageResolutionClient, IOutageResolutionContract>(serviceUri, servicePartitionKey);
        }

        #region IOutageResolutionContract
        public Task<bool> ResolveOutage(long outageId)
        {
            return InvokeWithRetryAsync(client => client.Channel.ResolveOutage(outageId));
        }

        public Task<bool> ValidateResolveConditions(long outageId)
        {
            return InvokeWithRetryAsync(client => client.Channel.ValidateResolveConditions(outageId));
        }

        public Task<bool> IsAlive()
        {
            return InvokeWithRetryAsync(client => client.Channel.IsAlive());
        }
        #endregion IOutageResolutionContract
    }
}

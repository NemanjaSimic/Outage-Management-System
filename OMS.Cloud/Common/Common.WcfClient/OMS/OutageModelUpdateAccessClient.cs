using Common.OmsContracts.ModelProvider;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Names;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.OMS
{
    public class OutageModelUpdateAccessClient : WcfSeviceFabricClientBase<IOutageModelUpdateAccessContract>, IOutageModelUpdateAccessContract
    {
        private static readonly string microserviceName = MicroserviceNames.OmsModelProviderService;
        private static readonly string listenerName = EndpointNames.OutageManagmenetServiceModelUpdateAccessEndpoint;
        public OutageModelUpdateAccessClient(WcfCommunicationClientFactory<IOutageModelUpdateAccessContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
            : base(clientFactory, serviceUri, servicePartition, listenerName)
        {

        }
        public static IOutageModelUpdateAccessContract CreateClient()
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<OutageModelUpdateAccessClient, IOutageModelUpdateAccessContract>(microserviceName);
        }

        public static IOutageModelUpdateAccessContract CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<OutageModelUpdateAccessClient, IOutageModelUpdateAccessContract>(serviceUri, servicePartitionKey);
        }
        public Task UpdateCommandedElements(long gid, ModelUpdateOperationType modelUpdateOperationType)
        {
            return InvokeWithRetryAsync(client => client.Channel.UpdateCommandedElements(gid, modelUpdateOperationType));
        }

        public Task UpdateOptimumIsolationPoints(long gid, ModelUpdateOperationType modelUpdateOperationType)
        {
            return InvokeWithRetryAsync(client => client.Channel.UpdateOptimumIsolationPoints(gid, modelUpdateOperationType));
        }

        public Task UpdatePotentialOutage(long gid, CommandOriginType commandOriginType, ModelUpdateOperationType modelUpdateOperationType)
        {
            return InvokeWithRetryAsync(client => client.Channel.UpdatePotentialOutage(gid, commandOriginType, modelUpdateOperationType));
        }
    }
}

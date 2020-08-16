using Common.OmsContracts.ModelProvider;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Names;
using OMS.Common.PubSubContracts.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.OMS
{
    public class OutageModelReadAccessClient : WcfSeviceFabricClientBase<IOutageModelReadAccessContract>, IOutageModelReadAccessContract
    {
        private static readonly string microserviceName = MicroserviceNames.OmsModelProviderService;
        private static readonly string listenerName = EndpointNames.OmsModelReadAccessEndpoint;
        public OutageModelReadAccessClient(WcfCommunicationClientFactory<IOutageModelReadAccessContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
            : base(clientFactory, serviceUri,servicePartition, listenerName)
        {

        }
        public static IOutageModelReadAccessContract CreateClient()
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<OutageModelReadAccessClient, IOutageModelReadAccessContract>(microserviceName);
        }

        public static IOutageModelReadAccessContract CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<OutageModelReadAccessClient, IOutageModelReadAccessContract>(serviceUri, servicePartitionKey);
        }

        public Task<Dictionary<long, long>> GetCommandedElements()
        {
            return InvokeWithRetryAsync(client => client.Channel.GetCommandedElements());
        }

		public Task<IOutageTopologyElement> GetElementById(long gid)
		{
            return InvokeWithRetryAsync(client => client.Channel.GetElementById(gid));
		}

		public Task<Dictionary<long, long>> GetOptimumIsolatioPoints()
        {
            return InvokeWithRetryAsync(client => client.Channel.GetOptimumIsolatioPoints());
        }

        public Task<Dictionary<long, CommandOriginType>> GetPotentialOutage()
        {
            return InvokeWithRetryAsync(client => client.Channel.GetPotentialOutage());
        }

        #region IOutageModelReadAccessContract
        public Task<IOutageTopologyModel> GetTopologyModel()
        {
            return InvokeWithRetryAsync(client => client.Channel.GetTopologyModel());
        }

        #endregion

        public Task<bool> IsAlive()
        {
            return InvokeWithRetryAsync(client => client.Channel.IsAlive());
        }
    }
}

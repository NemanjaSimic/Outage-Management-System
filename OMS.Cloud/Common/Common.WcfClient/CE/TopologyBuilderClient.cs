using Common.CeContracts;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using System;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.CE
{
    public class TopologyBuilderClient : WcfSeviceFabricClientBase<ITopologyBuilderContract>, ITopologyBuilderContract
	{
		private static readonly string microserviceName = MicroserviceNames.CeTopologyBuilderService;
		private static readonly string listenerName = EndpointNames.CeTopologyBuilderServiceEndpoint;
		public TopologyBuilderClient(WcfCommunicationClientFactory<ITopologyBuilderContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
			: base(clientFactory, serviceUri, servicePartition, listenerName)
		{

		}

		public static ITopologyBuilderContract CreateClient()
		{
			ClientFactory factory = new ClientFactory();
			return factory.CreateClient<TopologyBuilderClient, ITopologyBuilderContract>(microserviceName);
		}

		public static ITopologyBuilderContract CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
		{
			ClientFactory factory = new ClientFactory();
			return factory.CreateClient<TopologyBuilderClient, ITopologyBuilderContract>(serviceUri, servicePartitionKey);
		}

		public Task<TopologyModel> CreateGraphTopology(long firstElementGid, string whoIsCalling)
		{
			return InvokeWithRetryAsync(client => client.Channel.CreateGraphTopology(firstElementGid, whoIsCalling));
		}

		public Task<bool> IsAlive()
		{
			return InvokeWithRetryAsync(client => client.Channel.IsAlive());
		}
	}
}

using Common.CE.Interfaces;
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

		public static TopologyBuilderClient CreateClient()
		{
			ClientFactory factory = new ClientFactory();
			return factory.CreateClient<TopologyBuilderClient, ITopologyBuilderContract>(microserviceName);
		}

		public static TopologyBuilderClient CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
		{
			ClientFactory factory = new ClientFactory();
			return factory.CreateClient<TopologyBuilderClient, ITopologyBuilderContract>(serviceUri, servicePartitionKey);
		}

		public Task<ITopology> CreateGraphTopology(long firstElementGid)
		{
            return InvokeWithRetryAsync(client => client.Channel.CreateGraphTopology(firstElementGid));
		}
	}
}

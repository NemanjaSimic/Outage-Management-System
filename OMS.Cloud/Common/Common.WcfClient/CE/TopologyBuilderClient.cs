using CECommon.Interfaces;
using Common.CeContracts;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using System;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.CE
{
	public class TopologyBuilderClient : WcfSeviceFabricClientBase<ITopologyBuilderService>, ITopologyBuilderService
	{
		private static readonly string microserviceName = MicroserviceNames.TopologyBuilderService;
		private static readonly string listenerName = EndpointNames.TopologyBuilderServiceEndpoint;
		public TopologyBuilderClient(WcfCommunicationClientFactory<ITopologyBuilderService> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
			: base(clientFactory, serviceUri, servicePartition, listenerName)
		{

		}

		public static TopologyBuilderClient CreateClient(Uri serviceUri = null)
		{
			ClientFactory factory = new ClientFactory();
			ServicePartitionKey servicePartition = ServicePartitionKey.Singleton;

			if (serviceUri == null)
			{
				return factory.CreateClient<TopologyBuilderClient, ITopologyBuilderService>(microserviceName, servicePartition);
			}
			else
			{
				return factory.CreateClient<TopologyBuilderClient, ITopologyBuilderService>(serviceUri, servicePartition);
			}
		}
		public Task<ITopology> CreateGraphTopology(long firstElementGid)
		{
			return MethodWrapperAsync<ITopology>("CreateGraphTopology", new object[1] { firstElementGid});
		}
	}
}

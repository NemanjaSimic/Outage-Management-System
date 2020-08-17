using Common.CeContracts;
using Common.PubSubContracts.DataContracts.CE;
using Common.PubSubContracts.DataContracts.CE.UIModels;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using System;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.CE
{
    public class TopologyConverterClient : WcfSeviceFabricClientBase<ITopologyConverterContract>, ITopologyConverterContract
	{
		private static readonly string microserviceName = MicroserviceNames.CeTopologyProviderService;
		private static readonly string listenerName = EndpointNames.CeTopologyConverterServiceEndpoint;

		public TopologyConverterClient(WcfCommunicationClientFactory<ITopologyConverterContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
			: base(clientFactory, serviceUri, servicePartition, listenerName)
		{

		}

		public static ITopologyConverterContract CreateClient()
		{
			ClientFactory factory = new ClientFactory();
			return factory.CreateClient<TopologyConverterClient, ITopologyConverterContract>(microserviceName);
		}

		public static ITopologyConverterContract CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
		{
			ClientFactory factory = new ClientFactory();
			return factory.CreateClient<TopologyConverterClient, ITopologyConverterContract>(serviceUri, servicePartitionKey);
		}

		public Task<OutageTopologyModel> ConvertTopologyToOMSModel(TopologyModel topology)
		{
            return InvokeWithRetryAsync(client => client.Channel.ConvertTopologyToOMSModel(topology));
		}

		public Task<UIModel> ConvertTopologyToUIModel(TopologyModel topology)
		{
            return InvokeWithRetryAsync(client => client.Channel.ConvertTopologyToUIModel(topology));
		}

		public Task<bool> IsAlive()
		{
			return InvokeWithRetryAsync(client => client.Channel.IsAlive());
		}
	}
}

using Common.CE.Interfaces;
using Common.CeContracts;
using Common.CeContracts.TopologyProvider;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using OMS.Common.PubSub;
using System;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.CE
{
	public class TopologyProviderClient : WcfSeviceFabricClientBase<ITopologyProviderContract>, ITopologyProviderContract
	{
		private static readonly string microserviceName = MicroserviceNames.TopologyProviderService;
		private static readonly string listenerName = EndpointNames.TopologyProviderServiceEndpoint;

		public TopologyProviderClient(WcfCommunicationClientFactory<ITopologyProviderContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
			: base(clientFactory, serviceUri, servicePartition, listenerName)
		{

		}

		public static TopologyProviderClient CreateClient()
		{
			ClientFactory factory = new ClientFactory();
			return factory.CreateClient<TopologyProviderClient, ITopologyProviderContract>(microserviceName);
		}

		public static TopologyProviderClient CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
		{
			ClientFactory factory = new ClientFactory();
			return factory.CreateClient<TopologyProviderClient, ITopologyProviderContract>(serviceUri, servicePartitionKey);
		}


		public Task CommitTransaction()
		{
            return InvokeWithRetryAsync(client => client.Channel.CommitTransaction());
		}

		public Task DiscreteMeasurementDelegate()
		{
            return InvokeWithRetryAsync(client => client.Channel.DiscreteMeasurementDelegate());
		}

		public Task<IOutageTopologyModel> GetOMSModel()
		{
            return InvokeWithRetryAsync(client => client.Channel.GetOMSModel());
		}

		public Task<ITopology> GetTopology()
		{
            return InvokeWithRetryAsync(client => client.Channel.GetTopology());
		}

		public Task<UIModel> GetUIModel()
		{
            return InvokeWithRetryAsync(client => client.Channel.GetUIModel());
		}

		public Task<bool> IsElementRemote(long elementGid)
		{
            return InvokeWithRetryAsync(client => client.Channel.IsElementRemote(elementGid));
		}

		public Task<bool> PrepareForTransaction()
		{
            return InvokeWithRetryAsync(client => client.Channel.PrepareForTransaction());
		}

		public Task ResetRecloser(long recloserGid)
		{
            return InvokeWithRetryAsync(client => client.Channel.ResetRecloser(recloserGid));
		}

		public Task RollbackTransaction()
		{
            return InvokeWithRetryAsync(client => client.Channel.RollbackTransaction());
		}
	}
}

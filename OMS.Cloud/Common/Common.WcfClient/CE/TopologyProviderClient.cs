using Common.CeContracts;
using Common.CeContracts.TopologyProvider;
using Common.PubSubContracts.DataContracts.CE;
using Common.PubSubContracts.DataContracts.CE.UIModels;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using System;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.CE
{
    public class TopologyProviderClient : WcfSeviceFabricClientBase<ITopologyProviderContract>, ITopologyProviderContract
	{
		private static readonly string microserviceName = MicroserviceNames.CeTopologyProviderService;
		private static readonly string listenerName = EndpointNames.CeTopologyProviderServiceEndpoint;

		public TopologyProviderClient(WcfCommunicationClientFactory<ITopologyProviderContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
			: base(clientFactory, serviceUri, servicePartition, listenerName)
		{

		}

		public static ITopologyProviderContract CreateClient()
		{
			ClientFactory factory = new ClientFactory();
			return factory.CreateClient<TopologyProviderClient, ITopologyProviderContract>(microserviceName);
		}

		public static ITopologyProviderContract CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
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

		public Task<OutageTopologyModel> GetOMSModel()
		{
            return InvokeWithRetryAsync(client => client.Channel.GetOMSModel());
		}

		public Task<TopologyModel> GetTopology()
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

		public Task<bool> IsAlive()
		{
			return InvokeWithRetryAsync(client => client.Channel.IsAlive());
		}

		public Task RecloserOpened(long recloserGid)
		{
			return InvokeWithRetryAsync(client => client.Channel.RecloserOpened(recloserGid));

		}

		public Task<int> GetRecloserCount(long recloserGid)
		{
			return InvokeWithRetryAsync(client => client.Channel.GetRecloserCount(recloserGid));
		}
	}
}

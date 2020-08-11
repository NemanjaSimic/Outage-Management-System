using Common.CE.Interfaces;
using Common.CeContracts.ModelProvider;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.CE
{
	public class ModelProviderClient : WcfSeviceFabricClientBase<IModelProviderContract>, IModelProviderContract
	{
		private static readonly string microserviceName = MicroserviceNames.CeModelProviderService;
		private static readonly string listenerName = EndpointNames.CeModelProviderServiceEndpoint;
		public ModelProviderClient(WcfCommunicationClientFactory<IModelProviderContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition) 
            : base(clientFactory, serviceUri, servicePartition, listenerName)
		{

		}

		public static IModelProviderContract CreateClient()
		{
			ClientFactory factory = new ClientFactory();
			return factory.CreateClient<ModelProviderClient, IModelProviderContract>(microserviceName);
		}

		public static IModelProviderContract CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
		{
			ClientFactory factory = new ClientFactory();
			return factory.CreateClient<ModelProviderClient, IModelProviderContract>(serviceUri, servicePartitionKey);
		}

		public Task Commit()
		{
			return InvokeWithRetryAsync(client => client.Channel.Commit());
		}

		public Task<Dictionary<long, List<long>>> GetConnections()
		{
			return InvokeWithRetryAsync(client => client.Channel.GetConnections());
		}

		public Task<Dictionary<long, ITopologyElement>> GetElementModels()
		{
			return InvokeWithRetryAsync(client => client.Channel.GetElementModels());
		}

		public Task<List<long>> GetEnergySources()
		{
			return InvokeWithRetryAsync(client => client.Channel.GetEnergySources());
		}

		public Task<HashSet<long>> GetReclosers()
		{
			return InvokeWithRetryAsync(client => client.Channel.GetReclosers());
		}

		public Task<bool> IsAlive()
		{
			return InvokeWithRetryAsync(client => client.Channel.IsAlive());
		}

		public Task<bool> IsRecloser(long recloserGid)
		{
			return InvokeWithRetryAsync(client => client.Channel.IsRecloser(recloserGid));
		}

		public Task<bool> Prepare()
		{
			return InvokeWithRetryAsync(client => client.Channel.Prepare());
		}

		public Task Rollback()
		{
			return InvokeWithRetryAsync(client => client.Channel.Rollback());
		}

	}
}

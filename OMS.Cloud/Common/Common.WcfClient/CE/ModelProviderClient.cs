using CECommon.Interfaces;
using Common.CeContracts.ModelProvider;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.CE
{
	public class ModelProviderClient : WcfSeviceFabricClientBase<IModelProviderService>, IModelProviderService
	{
		private static readonly string microserviceName = MicroserviceNames.ModelProviderService;
		private static readonly string listenerName = EndpointNames.ModelProviderServiceEndpoint;
		public ModelProviderClient(WcfCommunicationClientFactory<IModelProviderService> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition) 
            : base(clientFactory, serviceUri, servicePartition, listenerName)
		{

		}

		public static ModelProviderClient CreateClient(Uri serviceUri = null)
		{
			ClientFactory factory = new ClientFactory();
			ServicePartitionKey servicePartition = ServicePartitionKey.Singleton;

			if (serviceUri == null)
			{
				return factory.CreateClient<ModelProviderClient, IModelProviderService>(microserviceName, servicePartition);
			}
			else
			{
				return factory.CreateClient<ModelProviderClient, IModelProviderService>(serviceUri, servicePartition);
			}
		}

		public Task CommitTransaction()
		{
			return MethodWrapperAsync("CommitTransaction", new object[0]);
			//return InvokeWithRetryAsync(client => client.Channel.CommitTransaction());
		}

		public Task<Dictionary<long, List<long>>> GetConnections()
		{
			return MethodWrapperAsync<Dictionary<long, List<long>>>("GetConnections", new object[0]);
			//return InvokeWithRetryAsync(client => client.Channel.GetConnections());
		}

		public Task<Dictionary<long, ITopologyElement>> GetElementModels()
		{
			return MethodWrapperAsync<Dictionary<long, ITopologyElement>>("GetElementModels", new object[0]);
			//return InvokeWithRetryAsync(client => client.Channel.GetElementModels());
		}

		public Task<List<long>> GetEnergySources()
		{
			return MethodWrapperAsync<List<long>>("GetEnergySources", new object[0]);
			//return InvokeWithRetryAsync(client => client.Channel.GetEnergySources());
		}

		public Task<HashSet<long>> GetReclosers()
		{
			return MethodWrapperAsync<HashSet<long>>("GetReclosers", new object[0]);
			//return InvokeWithRetryAsync(client => client.Channel.GetReclosers());
		}

		public Task<bool> IsRecloser(long recloserGid)
		{
			return MethodWrapperAsync<bool>("IsRecloser", new object[1] { recloserGid});
			//return InvokeWithRetryAsync(client => client.Channel.IsRecloser(recloserGid));
		}

		public Task<bool> PrepareForTransaction()
		{
			return MethodWrapperAsync<bool>("PrepareForTransaction", new object[0]);
			//return InvokeWithRetryAsync(client => client.Channel.PrepareForTransaction());
		}

		public Task RollbackTransaction()
		{
			return MethodWrapperAsync("RollbackTransaction", new object[0]);
			//return InvokeWithRetryAsync(client => client.Channel.RollbackTransaction());
		}

	}
}

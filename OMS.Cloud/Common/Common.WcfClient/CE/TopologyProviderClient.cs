using CECommon.CeContrats;
using CECommon.Interface;
using CECommon.Interfaces;
using Common.CeContracts.TopologyProvider;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using System;
using System.Collections.Generic;
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

		public static TopologyProviderClient CreateClient(Uri serviceUri = null)
		{
			ClientFactory factory = new ClientFactory();
			ServicePartitionKey servicePartition = ServicePartitionKey.Singleton;

			if (serviceUri == null)
			{
				return factory.CreateClient<TopologyProviderClient, ITopologyProviderContract>(microserviceName, servicePartition);
			}
			else
			{
				return factory.CreateClient<TopologyProviderClient, ITopologyProviderContract>(serviceUri, servicePartition);
			}
		}

		public Task CommitTransaction()
		{
			return MethodWrapperAsync("CommitTransaction", new object[0]);
		}

		public Task<IOutageTopologyModel> GetOMSModel()
		{
			return MethodWrapperAsync<IOutageTopologyModel>("GetOMSModel", new object[0]);
		}

		public Task<List<ITopology>> GetTopologies()
		{
			return MethodWrapperAsync<List<ITopology>>("GetTopologies", new object[0]);
		}

		public Task<UIModel> GetTopology()
		{
			return MethodWrapperAsync<UIModel>("GetTopology", new object[0]);
		}

		public Task<bool> IsElementRemote(long elementGid)
		{
			return MethodWrapperAsync<bool>("IsElementRemote", new object[1] { elementGid});
		}

		public Task<bool> PrepareForTransaction()
		{
			return MethodWrapperAsync<bool>("PrepareForTransaction", new object[0]);

		}

		public Task ResetRecloser(long recloserGid)
		{
			return MethodWrapperAsync("ResetRecloser", new object[1] { recloserGid});
		}

		public Task RollbackTransaction()
		{
			return MethodWrapperAsync("RollbackTransaction", new object[0]);
		}
	}
}

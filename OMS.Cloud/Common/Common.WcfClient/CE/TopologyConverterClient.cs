using CECommon;
using CECommon.Interface;
using CECommon.Interfaces;
using Common.CeContracts;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using System;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.CE
{
	public class TopologyConverterClient : WcfSeviceFabricClientBase<ITopologyConverterContract>, ITopologyConverterContract
	{
		private static readonly string microserviceName = MicroserviceNames.TopologyConverterService;
		private static readonly string listenerName = EndpointNames.TopologyConverterServiceEndpoint;

		public TopologyConverterClient(WcfCommunicationClientFactory<ITopologyConverterContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
			: base(clientFactory, serviceUri, servicePartition, listenerName)
		{

		}

		public static TopologyConverterClient CreateClient(Uri serviceUri = null)
		{
			ClientFactory factory = new ClientFactory();
			ServicePartitionKey servicePartition = ServicePartitionKey.Singleton;

			if (serviceUri == null)
			{
				return factory.CreateClient<TopologyConverterClient, ITopologyConverterContract>(microserviceName, servicePartition);
			}
			else
			{
				return factory.CreateClient<TopologyConverterClient, ITopologyConverterContract>(serviceUri, servicePartition);
			}
		}

		public Task<IOutageTopologyModel> ConvertTopologyToOMSModel(ITopology topology)
		{
			return MethodWrapperAsync<IOutageTopologyModel>("ConvertTopologyToOMSModel", new object[1] { topology});
		}

		public Task<UIModel> ConvertTopologyToUIModel(ITopology topology)
		{
			return MethodWrapperAsync<UIModel>("ConvertTopologyToUIModel", new object[1] { topology });
		}
	}
}

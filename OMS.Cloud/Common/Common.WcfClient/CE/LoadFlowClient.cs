using CECommon.Interfaces;
using Common.CeContracts.LoadFlow;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.CE
{
	public class LoadFlowClient : WcfSeviceFabricClientBase<ILoadFlowService>, ILoadFlowService
	{
		private static readonly string microserviceName = MicroserviceNames.LoadFlowService;
		private static readonly string listenerName = EndpointNames.LoadFlowServiceEndpoint;
		public LoadFlowClient(WcfCommunicationClientFactory<ILoadFlowService> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
			: base(clientFactory, serviceUri, servicePartition, listenerName)
		{

		}

		public static LoadFlowClient CreateClient(Uri serviceUri = null)
		{
			ClientFactory factory = new ClientFactory();
			ServicePartitionKey servicePartition = ServicePartitionKey.Singleton;

			if (serviceUri == null)
			{
				return factory.CreateClient<LoadFlowClient, ILoadFlowService>(microserviceName, servicePartition);
			}
			else
			{
				return factory.CreateClient<LoadFlowClient, ILoadFlowService>(serviceUri, servicePartition);
			}
		}
		public Task UpdateLoadFlow(List<ITopology> topologies)
		{
			return MethodWrapperAsync("UpdateLoadFlow", new object[1] { topologies });
		}
	}
}

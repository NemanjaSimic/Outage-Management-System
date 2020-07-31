using Common.CeContracts;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using System;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.CE
{
	public class SwitchStatusCommandingClient : WcfSeviceFabricClientBase<ISwitchStatusCommandingContract>, ISwitchStatusCommandingContract
	{
		private static readonly string microserviceName = MicroserviceNames.SwitchStatusCommandingService;
		private static readonly string listenerName = EndpointNames.SwitchStatusCommandingEndpoint;

		public SwitchStatusCommandingClient(WcfCommunicationClientFactory<ISwitchStatusCommandingContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
			: base(clientFactory, serviceUri, servicePartition, listenerName)
		{

		}

		public static SwitchStatusCommandingClient CreateClient(Uri serviceUri = null)
		{
			ClientFactory factory = new ClientFactory();
			ServicePartitionKey servicePartition = ServicePartitionKey.Singleton;

			if (serviceUri == null)
			{
				return factory.CreateClient<SwitchStatusCommandingClient, ISwitchStatusCommandingContract>(microserviceName, servicePartition);
			}
			else
			{
				return factory.CreateClient<SwitchStatusCommandingClient, ISwitchStatusCommandingContract>(serviceUri, servicePartition);
			}
		}

		public Task SendCloseCommand(long gid)
		{
			return MethodWrapperAsync("SendCloseCommand", new object[0]);

		}

		public Task SendOpenCommand(long gid)
		{
			return MethodWrapperAsync("SendOpenCommand", new object[0]);

		}
	}
}

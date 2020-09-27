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
		private static readonly string microserviceName = MicroserviceNames.CeMeasurementProviderService;
		private static readonly string listenerName = EndpointNames.CeSwitchStatusCommandingEndpoint;

		public SwitchStatusCommandingClient(WcfCommunicationClientFactory<ISwitchStatusCommandingContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
			: base(clientFactory, serviceUri, servicePartition, listenerName)
		{

		}

		public static ISwitchStatusCommandingContract CreateClient()
		{
			ClientFactory factory = new ClientFactory();
			return factory.CreateClient<SwitchStatusCommandingClient, ISwitchStatusCommandingContract>(microserviceName);
		}

		public static ISwitchStatusCommandingContract CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
		{
			ClientFactory factory = new ClientFactory();
			return factory.CreateClient<SwitchStatusCommandingClient, ISwitchStatusCommandingContract>(serviceUri, servicePartitionKey);
		}

		public Task<bool> SendCloseCommand(long gid)
		{
            return InvokeWithRetryAsync(client => client.Channel.SendCloseCommand(gid));
		}

		public Task<bool> SendOpenCommand(long gid)
		{
            return InvokeWithRetryAsync(client => client.Channel.SendOpenCommand(gid));
		}

		public Task<bool> IsAlive()
		{
			return InvokeWithRetryAsync(client => client.Channel.IsAlive());
		}
	}
}

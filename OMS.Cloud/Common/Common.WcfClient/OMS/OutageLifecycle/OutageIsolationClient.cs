using Common.OmsContracts.OutageLifecycle;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using System;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.OMS.OutageLifecycle
{
    public class OutageIsolationClient : WcfSeviceFabricClientBase<IOutageIsolationContract>, IOutageIsolationContract
	{
		private static readonly string microserviceName = MicroserviceNames.OmsOutageLifecycleService;
		private static readonly string listenerName = EndpointNames.OmsOutageIsolationEndpoint;

		public OutageIsolationClient(WcfCommunicationClientFactory<IOutageIsolationContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
			: base(clientFactory, serviceUri, servicePartition, listenerName)
		{
		}

		public static IOutageIsolationContract CreateClient()
		{
			ClientFactory factory = new ClientFactory();
			return factory.CreateClient<OutageIsolationClient, IOutageIsolationContract>(microserviceName);
		}

		public static IOutageIsolationContract CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
		{
			ClientFactory factory = new ClientFactory();
			return factory.CreateClient<OutageIsolationClient, IOutageIsolationContract>(serviceUri, servicePartitionKey);
		}

		#region IOutageIsolationContract
		public Task IsolateOutage(long outageId)
        {
			return InvokeWithRetryAsync(client => client.Channel.IsolateOutage(outageId));
        }

		public Task<bool> IsAlive()
		{
			return InvokeWithRetryAsync(client => client.Channel.IsAlive());
		}
		#endregion IOutageIsolationContract
	}
}

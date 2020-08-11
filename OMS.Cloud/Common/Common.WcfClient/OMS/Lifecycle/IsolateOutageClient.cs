using Common.OmsContracts.OutageLifecycle;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.OMS.Lifecycle
{
    public class IsolateOutageClient : WcfSeviceFabricClientBase<IIsolateOutageContract>, IIsolateOutageContract
	{
		private static readonly string microserviceName = MicroserviceNames.OmsOutageLifecycleService;
		private static readonly string listenerName = EndpointNames.IsolateOutageEndpoint;
		public IsolateOutageClient(WcfCommunicationClientFactory<IIsolateOutageContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
			: base(clientFactory, serviceUri, servicePartition, listenerName)
		{

		}

		public static IIsolateOutageContract CreateClient()
		{
			ClientFactory factory = new ClientFactory();
			return factory.CreateClient<IsolateOutageClient, IIsolateOutageContract>(microserviceName);
		}

		public static IIsolateOutageContract CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
		{
			ClientFactory factory = new ClientFactory();
			return factory.CreateClient<IsolateOutageClient, IIsolateOutageContract>(serviceUri, servicePartitionKey);
		}

        public Task IsolateOutage(long outageId)
        {
			return InvokeWithRetryAsync(client => client.Channel.IsolateOutage(outageId));
        }

		public Task<bool> IsAlive()
		{
			return InvokeWithRetryAsync(client => client.Channel.IsAlive());
		}
	}
}

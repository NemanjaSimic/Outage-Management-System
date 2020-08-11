using Common.OmsContracts.OutageLifecycle;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Names;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.OMS.Lifecycle
{
	public class ReportOutageClient : WcfSeviceFabricClientBase<IReportOutageContract>, IReportOutageContract
	{
		private static readonly string microserviceName = MicroserviceNames.OmsOutageLifecycleService;
		private static readonly string listenerName = EndpointNames.ReportOutageEndpoint;
		public ReportOutageClient(WcfCommunicationClientFactory<IReportOutageContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition) 
			: base (clientFactory, serviceUri, servicePartition, listenerName)
		{

		}

		public static IReportOutageContract CreateClient()
		{
			ClientFactory factory = new ClientFactory();
			return factory.CreateClient<ReportOutageClient, IReportOutageContract>(microserviceName);
		}

		public static IReportOutageContract CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
		{
			ClientFactory factory = new ClientFactory();
			return factory.CreateClient<ReportOutageClient, IReportOutageContract>(serviceUri, servicePartitionKey);
		}

		public Task<bool> ReportPotentialOutage(long gid, CommandOriginType commandOriginType)
		{
			return InvokeWithRetryAsync(client => client.Channel.ReportPotentialOutage(gid, commandOriginType));
		}

		public Task<bool> IsAlive()
		{
			return InvokeWithRetryAsync(client => client.Channel.IsAlive());
		}
	}
}

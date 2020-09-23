using Common.OmsContracts.OutageLifecycle;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Names;
using System;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.OMS.OutageLifecycle
{
	public class PotentialOutageReportingClient : WcfSeviceFabricClientBase<IPotentialOutageReportingContract>, IPotentialOutageReportingContract
	{
		private static readonly string microserviceName = MicroserviceNames.OmsOutageLifecycleService;
		private static readonly string listenerName = EndpointNames.OmsPotentialOutageReportingEndpoint;

		public PotentialOutageReportingClient(WcfCommunicationClientFactory<IPotentialOutageReportingContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition) 
			: base (clientFactory, serviceUri, servicePartition, listenerName)
		{
		}

		public static IPotentialOutageReportingContract CreateClient()
		{
			ClientFactory factory = new ClientFactory();
			return factory.CreateClient<PotentialOutageReportingClient, IPotentialOutageReportingContract>(microserviceName);
		}

		public static IPotentialOutageReportingContract CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
		{
			ClientFactory factory = new ClientFactory();
			return factory.CreateClient<PotentialOutageReportingClient, IPotentialOutageReportingContract>(serviceUri, servicePartitionKey);
		}

		#region IPotentialOutageReportingContract
		public Task<bool> EnqueuePotentialOutageCommand(long elementGid, CommandOriginType commandOriginType)
		{
			return InvokeWithRetryAsync(client => client.Channel.EnqueuePotentialOutageCommand(elementGid, commandOriginType));
		}

		public Task<bool> ReportPotentialOutage(long elementGid, CommandOriginType commandOriginType)
		{
			return InvokeWithRetryAsync(client => client.Channel.ReportPotentialOutage(elementGid, commandOriginType));
		}

		public Task<bool> IsAlive()
		{
			return InvokeWithRetryAsync(client => client.Channel.IsAlive());
		}
        #endregion IPotentialOutageReportingContract
    }
}

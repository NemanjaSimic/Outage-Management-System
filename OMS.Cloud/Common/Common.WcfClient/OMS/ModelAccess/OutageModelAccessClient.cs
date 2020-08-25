using Common.OmsContracts.DataContracts.OutageDatabaseModel;
using Common.OmsContracts.ModelAccess;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.OMS.ModelAccess
{
    public class OutageModelAccessClient : WcfSeviceFabricClientBase<IOutageAccessContract>, IOutageAccessContract
    {
        private static readonly string microserviceName = MicroserviceNames.OmsHistoryDBManagerService;
        private static readonly string listenerName = EndpointNames.OmsOutageAccessEndpoint;
        public OutageModelAccessClient(WcfCommunicationClientFactory<IOutageAccessContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
           : base(clientFactory, serviceUri, servicePartition, listenerName)
        {

        }
        public static IOutageAccessContract CreateClient()
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<OutageModelAccessClient, IOutageAccessContract>(microserviceName);
        }

        public static IOutageAccessContract CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<OutageModelAccessClient, IOutageAccessContract>(serviceUri, servicePartitionKey);
        }

		public Task<OutageEntity> AddOutage(OutageEntity outage)
		{
			return InvokeWithRetryAsync(client => client.Channel.AddOutage(outage));
		}

        public Task<IEnumerable<OutageEntity>> FindOutage(OutageExpression expression)
        {
            return InvokeWithRetryAsync(client => client.Channel.FindOutage(expression));
        }

        public Task<IEnumerable<OutageEntity>> GetAllActiveOutages()
		{
			return InvokeWithRetryAsync(client => client.Channel.GetAllActiveOutages());
		}

		public Task<IEnumerable<OutageEntity>> GetAllArchivedOutages()
		{
			return InvokeWithRetryAsync(client => client.Channel.GetAllArchivedOutages());
		}

		public Task<IEnumerable<OutageEntity>> GetAllOutages()
		{
			return InvokeWithRetryAsync(client => client.Channel.GetAllOutages());
		}

		public Task<OutageEntity> GetOutage(long gid)
		{
			return InvokeWithRetryAsync(client => client.Channel.GetOutage(gid));
		}

		public Task RemoveAllOutages()
		{
			return InvokeWithRetryAsync(client => client.Channel.RemoveAllOutages());
		}

		public Task RemoveOutage(OutageEntity outage)
		{
			return InvokeWithRetryAsync(client => client.Channel.RemoveOutage(outage));
		}

		public Task UpdateOutage(OutageEntity outage)
		{
			return InvokeWithRetryAsync(client => client.Channel.UpdateOutage(outage));
		}

		public Task<bool> IsAlive()
		{
			return InvokeWithRetryAsync(client => client.Channel.IsAlive());
		}
	}
}

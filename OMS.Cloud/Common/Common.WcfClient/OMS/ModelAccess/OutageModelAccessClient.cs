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

		#region IOutageAccessContract
		public Task<OutageEntity> AddOutage(OutageEntity outage)
		{
			return InvokeWithRetryAsync(client => client.Channel.AddOutage(outage));
		}

        public Task<List<OutageEntity>> GetAllActiveOutages()
		{
			return InvokeWithRetryAsync(client => client.Channel.GetAllActiveOutages());
		}

		public Task<List<OutageEntity>> GetAllArchivedOutages()
		{
			return InvokeWithRetryAsync(client => client.Channel.GetAllArchivedOutages());
		}

		public Task<List<OutageEntity>> GetAllOutages()
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
		#endregion IOutageAccessContract
	}
}

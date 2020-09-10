using Common.OmsContracts.OutageLifecycle;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using System;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.OMS.OutageLifecycle
{
    public class CrewSendingClient : WcfSeviceFabricClientBase<ICrewSendingContract>, ICrewSendingContract
    {
        private static readonly string microserviceName = MicroserviceNames.OmsOutageLifecycleService;
        private static readonly string listenerName = EndpointNames.OmsCrewSendingEndpoint;

        public CrewSendingClient(WcfCommunicationClientFactory<ICrewSendingContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
           : base(clientFactory, serviceUri, servicePartition, listenerName)
        {

        }
        public static ICrewSendingContract CreateClient()
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<CrewSendingClient, ICrewSendingContract>(microserviceName);
        }

        public static ICrewSendingContract CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<CrewSendingClient, ICrewSendingContract>(serviceUri, servicePartitionKey);
        }

        #region ICrewSendingContract
        public Task<bool> SendLocationIsolationCrew(long outageId)
        {
            return InvokeWithRetryAsync(client => client.Channel.SendLocationIsolationCrew(outageId));
        }
        
        public Task<bool> SendRepairCrew(long outageId)
        {
            return InvokeWithRetryAsync(client => client.Channel.SendRepairCrew(outageId));
        }

        public Task<bool> IsAlive()
        {
            return InvokeWithRetryAsync(client => client.Channel.IsAlive());
        }
        #endregion ICrewSendingContract
    }
}

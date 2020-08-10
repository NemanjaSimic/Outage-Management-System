using Common.OmsContracts.OutageLifecycle;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.OMS
{
    public class SendLocationIsolationCrewClient : WcfSeviceFabricClientBase<ISendLocationIsolationCrewContract>, ISendLocationIsolationCrewContract
    {
        private static readonly string microserviceName = MicroserviceNames.OmsOutageLifecycleService;
        private static readonly string listenerName = EndpointNames.SendLocationIsolationCrewEndpoint;
        public SendLocationIsolationCrewClient(WcfCommunicationClientFactory<ISendLocationIsolationCrewContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
           : base(clientFactory, serviceUri, servicePartition, listenerName)
        {

        }

        public static ISendLocationIsolationCrewContract CreateClient()
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<SendLocationIsolationCrewClient, ISendLocationIsolationCrewContract>(microserviceName);
        }

        public static ISendLocationIsolationCrewContract CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<SendLocationIsolationCrewClient, ISendLocationIsolationCrewContract>(serviceUri, servicePartitionKey);
        }
        public Task<bool> SendLocationIsolationCrew(long outageId)
        {
            return InvokeWithRetryAsync(client => client.Channel.SendLocationIsolationCrew(outageId));
        }
    }
}

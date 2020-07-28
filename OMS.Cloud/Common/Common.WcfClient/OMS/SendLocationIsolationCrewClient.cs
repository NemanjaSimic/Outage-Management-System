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

        public static SendLocationIsolationCrewClient CreateClient(Uri serviceUri = null)
        {
            ClientFactory factory = new ClientFactory();
            ServicePartitionKey servicePartition = ServicePartitionKey.Singleton;

            if (serviceUri == null)
            {
                return factory.CreateClient<SendLocationIsolationCrewClient, ISendLocationIsolationCrewContract>(microserviceName, servicePartition);
            }
            else
            {
                return factory.CreateClient<SendLocationIsolationCrewClient, ISendLocationIsolationCrewContract>(serviceUri, servicePartition);
            }
        }
        public Task<bool> SendLocationIsolationCrew(long outageId)
        {
            return InvokeWithRetryAsync(client => client.Channel.SendLocationIsolationCrew(outageId));
        }
    }
}

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
    public class SendRepairCrewClient : WcfSeviceFabricClientBase<ISendRepairCrewContract>, ISendRepairCrewContract
    {
        private static readonly string microserviceName = MicroserviceNames.OmsOutageLifecycleService;
        private static readonly string listenerName = EndpointNames.OmsSendRepairCrewEndpoint;
        public SendRepairCrewClient(WcfCommunicationClientFactory<ISendRepairCrewContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
           : base(clientFactory, serviceUri, servicePartition, listenerName)
        {

        }
        public static ISendRepairCrewContract CreateClient()
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<SendRepairCrewClient, ISendRepairCrewContract>(microserviceName);
        }

        public static ISendRepairCrewContract CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<SendRepairCrewClient, ISendRepairCrewContract>(serviceUri, servicePartitionKey);
        }
        public Task<bool> SendRepairCrew(long outageId)
        {
            return InvokeWithRetryAsync(client => client.Channel.SendRepairCrew(outageId));
        }

        public Task<bool> IsAlive()
        {
            return InvokeWithRetryAsync(client => client.Channel.IsAlive());
        }
    }
}

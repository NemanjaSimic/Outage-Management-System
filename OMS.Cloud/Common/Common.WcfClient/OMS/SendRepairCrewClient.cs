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
        private static readonly string listenerName = EndpointNames.SendRepairCrewEndpoint;
        public SendRepairCrewClient(WcfCommunicationClientFactory<ISendRepairCrewContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
           : base(clientFactory, serviceUri, servicePartition, listenerName)
        {

        }
        public static SendRepairCrewClient CreateClient(Uri serviceUri = null)
        {
            ClientFactory factory = new ClientFactory();
            ServicePartitionKey servicePartition = ServicePartitionKey.Singleton;

            if (serviceUri == null)
            {
                return factory.CreateClient<SendRepairCrewClient, ISendRepairCrewContract>(microserviceName, servicePartition);
            }
            else
            {
                return factory.CreateClient<SendRepairCrewClient, ISendRepairCrewContract>(serviceUri, servicePartition);
            }
        }
        public Task<bool> SendRepairCrew(long outageId)
        {
            return InvokeWithRetryAsync(client => client.Channel.SendRepairCrew(outageId));
        }
    }
}

using Common.OmsContracts.OutageSimulator;
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
    public class OutageSimulatorClient : WcfSeviceFabricClientBase<IOutageSimulatorContract>, IOutageSimulatorContract
    {
        private static readonly string microserviceName = MicroserviceNames.OmsOutageSimulatorService;
        private static readonly string listenerName = EndpointNames.OmsOutageSimulatorEndpoint;
        
        public OutageSimulatorClient(WcfCommunicationClientFactory<IOutageSimulatorContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
           : base(clientFactory, serviceUri, servicePartition, listenerName)
        {

        }

        public static IOutageSimulatorContract CreateClient()
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<OutageSimulatorClient, IOutageSimulatorContract>(microserviceName);
        }

        public static IOutageSimulatorContract CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<OutageSimulatorClient, IOutageSimulatorContract>(serviceUri, servicePartitionKey);
        }

        public Task<bool> IsOutageElement(long outageElementId)
        {
            return InvokeWithRetryAsync(client => client.Channel.IsOutageElement(outageElementId));
        }

        public Task<bool> StopOutageSimulation(long outageElementId)
        {
            return InvokeWithRetryAsync(client => client.Channel.StopOutageSimulation(outageElementId));
        }

        public Task<bool> IsAlive()
        {
            return InvokeWithRetryAsync(client => client.Channel.IsAlive());
        }
    }
}

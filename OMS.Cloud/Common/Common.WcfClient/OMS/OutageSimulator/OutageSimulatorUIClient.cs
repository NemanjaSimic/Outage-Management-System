using Common.OmsContracts.DataContracts.OutageSimulator;
using Common.OmsContracts.OutageSimulator;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.OMS.OutageSimulator
{
    public class OutageSimulatorUIClient : WcfSeviceFabricClientBase<IOutageSimulatorUIContract>, IOutageSimulatorUIContract
    {
        private static readonly string microserviceName = MicroserviceNames.OmsOutageSimulatorService;
        private static readonly string listenerName = EndpointNames.OmsOutageSimulatorUIEndpoint;

        public OutageSimulatorUIClient(WcfCommunicationClientFactory<IOutageSimulatorUIContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
           : base(clientFactory, serviceUri, servicePartition, listenerName)
        {

        }

        public static IOutageSimulatorUIContract CreateClient()
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<OutageSimulatorUIClient, IOutageSimulatorUIContract>(microserviceName);
        }

        public static IOutageSimulatorUIContract CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<OutageSimulatorUIClient, IOutageSimulatorUIContract>(serviceUri, servicePartitionKey);
        }

        #region IOutageSimulatorUIContract
        public Task<IEnumerable<SimulatedOutage>> GetAllSimulatedOutages()
        {
            return InvokeWithRetryAsync(client => client.Channel.GetAllSimulatedOutages());
        }

        public Task<bool> GenerateOutage(SimulatedOutage outage)
        {
            return InvokeWithRetryAsync(client => client.Channel.GenerateOutage(outage));
        }

        public Task<bool> EndOutage(long outageElementGid)
        {
            return InvokeWithRetryAsync(client => client.Channel.EndOutage(outageElementGid));
        }

        public Task<bool> IsAlive()
        {
            return InvokeWithRetryAsync(client => client.Channel.IsAlive());
        }
        #endregion
    }
}

using Common.OmsContracts;
using Common.PubSubContracts.DataContracts.OMS;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.OMS
{
    public class OutageAccessClient : WcfSeviceFabricClientBase<IOutageAccessContract>, IOutageAccessContract
    {
        private static readonly string microserviceName = "TODO: BOBO, OSMOTRI :-)";
        private static readonly string listenerName = "TODO: BOBO, OSMOTRI :-)";
        public OutageAccessClient(WcfCommunicationClientFactory<IOutageAccessContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
            : base(clientFactory, serviceUri, servicePartition, listenerName)
        {

        }
        public static IOutageAccessContract CreateClient()
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<OutageAccessClient, IOutageAccessContract>(microserviceName);
        }

        public static IOutageAccessContract CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<OutageAccessClient, IOutageAccessContract>(serviceUri, servicePartitionKey);
        }

        public Task<IEnumerable<ActiveOutageMessage>> GetActiveOutages()
        {
            return InvokeWithRetryAsync(client => client.Channel.GetActiveOutages());
        }

        public Task<IEnumerable<ArchivedOutageMessage>> GetArchivedOutages()
        {
            return InvokeWithRetryAsync(client => client.Channel.GetArchivedOutages());
        }
    }
}

using Common.OmsContracts.HistoryDBManager;
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
    public class HistoryDBManagerClient : WcfSeviceFabricClientBase<IHistoryDBManagerContract>, IHistoryDBManagerContract
    {
        private static readonly string microserviceName = MicroserviceNames.OmsHistoryDBManagerService;
        private static readonly string listenerName = EndpointNames.HistoryDBManagerEndpoint;
        public HistoryDBManagerClient(WcfCommunicationClientFactory<IHistoryDBManagerContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
           : base(clientFactory, serviceUri, servicePartition, listenerName)
        {

        }
        public static HistoryDBManagerClient CreateClient(Uri serviceUri = null)
        {
            ClientFactory factory = new ClientFactory();
            ServicePartitionKey servicePartition = new ServicePartitionKey(0);
            if (serviceUri == null)
            {
                return factory.CreateClient<HistoryDBManagerClient, IHistoryDBManagerContract>(microserviceName, servicePartition);
            }
            else
            {
                return factory.CreateClient<HistoryDBManagerClient, IHistoryDBManagerContract>(serviceUri, servicePartition);
            }
        }
        public Task OnConsumerBlackedOut(List<long> consumers, long? outageId)
        {
            return InvokeWithRetryAsync(client => client.Channel.OnConsumerBlackedOut(consumers, outageId));
        }

        public Task OnConsumersEnergized(HashSet<long> consumers)
        {
            return InvokeWithRetryAsync(client => client.Channel.OnConsumersEnergized(consumers));
        }

        public Task OnSwitchClosed(long elementGid)
        {
            return InvokeWithRetryAsync(client => client.Channel.OnSwitchClosed(elementGid));
        }

        public Task OnSwitchOpened(long elementGid, long? outageId)
        {
            return InvokeWithRetryAsync(client => client.Channel.OnSwitchOpened(elementGid, outageId));
        }
    }
}

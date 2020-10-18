using Common.OmsContracts.HistoryDBManager;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.OMS.HistoryDBManager
{
    public class HistoryDBManagerClient : WcfSeviceFabricClientBase<IHistoryDBManagerContract>, IHistoryDBManagerContract
    {
        private static readonly string microserviceName = MicroserviceNames.OmsHistoryDBManagerService;
        private static readonly string listenerName = EndpointNames.OmsHistoryDBManagerEndpoint;
        public HistoryDBManagerClient(WcfCommunicationClientFactory<IHistoryDBManagerContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
           : base(clientFactory, serviceUri, servicePartition, listenerName)
        {

        }
        public static IHistoryDBManagerContract CreateClient()
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<HistoryDBManagerClient, IHistoryDBManagerContract>(microserviceName);
        }

        public static IHistoryDBManagerContract CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<HistoryDBManagerClient, IHistoryDBManagerContract>(serviceUri, servicePartitionKey);
        }

        #region IHistoryDBManagerContract
        //public Task OnConsumerBlackedOut(List<long> consumers, long? outageId)
        //{
        //    return InvokeWithRetryAsync(client => client.Channel.OnConsumerBlackedOut(consumers, outageId));
        //}

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

        public Task<bool> IsAlive()
        {
            return InvokeWithRetryAsync(client => client.Channel.IsAlive());
        }

        public Task OnConsumerBlackedOut(long consumer, long? outageId)
        {
            return InvokeWithRetryAsync(client => client.Channel.OnConsumerBlackedOut(consumer, outageId));

        }

        public Task UpdateClosedSwitch(long elementGid, long outageId)
        {
            return InvokeWithRetryAsync(client => client.Channel.UpdateClosedSwitch(elementGid, outageId));

        }

        public Task UpdateConsumer(long consumer, long outageId)
        {
            return InvokeWithRetryAsync(client => client.Channel.UpdateConsumer(consumer, outageId));

        }
        #endregion IHistoryDBManagerContract
    }
}

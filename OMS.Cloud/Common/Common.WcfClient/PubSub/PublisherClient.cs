using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using OMS.Common.PubSubContracts;
using OMS.Common.PubSubContracts.Interfaces;
using System;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.PubSub
{
    public class PublisherClient : WcfSeviceFabricClientBase<IPublisherContract>, IPublisherContract
	{
        private static readonly string microserviceName = MicroserviceNames.PubSubService;
        private static readonly string listenerName = EndpointNames.PubSubPublisherEndpoint;

        public PublisherClient(WcfCommunicationClientFactory<IPublisherContract> clientFactory, Uri serviceName, ServicePartitionKey servicePartition)
            : base(clientFactory, serviceName, servicePartition, listenerName)
        {
        }

        public static IPublisherContract CreateClient()
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<PublisherClient, IPublisherContract>(microserviceName);
        }

        public static IPublisherContract CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<PublisherClient, IPublisherContract>(serviceUri, servicePartitionKey);
        }

        #region ISubscriberContract
        public Task<bool> Publish(IPublication publication, string publisherName)
        {
            //return MethodWrapperAsync<bool>("Publish", new object[2] { publication, publisherName });
            return InvokeWithRetryAsync(client => client.Channel.Publish(publication, publisherName));
        }
        #endregion ISubscriberContract

        public Task<bool> IsAlive()
        {
            return InvokeWithRetryAsync(client => client.Channel.IsAlive());
        }
    }
}

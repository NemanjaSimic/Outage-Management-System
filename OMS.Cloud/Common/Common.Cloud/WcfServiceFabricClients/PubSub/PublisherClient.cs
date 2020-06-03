using Common.PubSub;
using Common.PubSubContracts;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using Outage.Common;
using System;
using System.Threading.Tasks;

namespace OMS.Common.Cloud.WcfServiceFabricClients.PubSub
{
    public class PublisherClient : WcfSeviceFabricClientBase<IPublisherContract>, IPublisherContract
    {
        private static readonly string microserviceName = MicroserviceNames.PubSubService;
        private static readonly string listenerName = EndpointNames.PublisherEndpoint;

        public PublisherClient(WcfCommunicationClientFactory<IPublisherContract> clientFactory, Uri serviceName, ServicePartitionKey servicePartition)
            : base(clientFactory, serviceName, servicePartition, listenerName)
        {
        }

        public static PublisherClient CreateClient(Uri serviceUri = null)
        {
            ClientFactory factory = new ClientFactory();
            ServicePartitionKey servicePartition = new ServicePartitionKey(0);

            if (serviceUri == null)
            {
                return factory.CreateClient<PublisherClient, IPublisherContract>(microserviceName, servicePartition);
            }
            else
            {
                return factory.CreateClient<PublisherClient, IPublisherContract>(serviceUri, servicePartition);
            }
        }

        #region ISubscriberContract
        public Task<bool> Publish(IPublication publication, Uri publisherUri)
        {
            return InvokeWithRetryAsync(client => client.Channel.Publish(publication, publisherUri));
        }
        #endregion ISubscriberContract
    }
}

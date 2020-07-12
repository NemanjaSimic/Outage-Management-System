using Common.PubSub;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Names;
using OMS.Common.PubSubContracts;
using System;
using System.Collections.Generic;
using System.Configuration;
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

        public static PublisherClient CreateClient()
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<PublisherClient, IPublisherContract>(microserviceName);
        }

        public static PublisherClient CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<PublisherClient, IPublisherContract>(serviceUri, servicePartitionKey);
        }

        #region ISubscriberContract
        public Task<bool> Publish(IPublication publication, string publisherName)
        {
            return MethodWrapperAsync<bool>("Publish", new object[2] { publication, publisherName });
            //return InvokeWithRetryAsync(client => client.Channel.Publish(publication, publisherUri));
        }
        #endregion ISubscriberContract
    }
}

using Common.PubSub;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
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
        public Task<bool> Publish(IPublication publication, Uri publisherUri = null)
        {
            if (publisherUri == null)
            {
                string appSettingsKey = "PublisherUriString";
                if (ConfigurationManager.AppSettings[appSettingsKey] is string publisherUriString)
                {
                    publisherUri = new Uri(publisherUriString);
                }
                else
                {
                    throw new KeyNotFoundException($"Key '{appSettingsKey}' not found in app.config - Example for publisher in ModelProviderService: <add key=\"PublisherUriString\" value=\"fabric:/ OMS.Cloud / SCADA.ModelProviderService\" />.");
                }
            }

            return InvokeWithRetryAsync(client => client.Channel.Publish(publication, publisherUri));
        }
        #endregion ISubscriberContract
    }
}

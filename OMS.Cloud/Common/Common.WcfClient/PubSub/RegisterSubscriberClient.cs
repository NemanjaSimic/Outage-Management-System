using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Names;
using OMS.Common.PubSubContracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.PubSub
{
    public class RegisterSubscriberClient : WcfSeviceFabricClientBase<IRegisterSubscriberContract>, IRegisterSubscriberContract
    {
        private static readonly string microserviceName = MicroserviceNames.PubSubService;
        private static readonly string listenerName = EndpointNames.SubscriberEndpoint; 

        public RegisterSubscriberClient(WcfCommunicationClientFactory<IRegisterSubscriberContract> clientFactory, Uri serviceName, ServicePartitionKey servicePartition) 
            : base(clientFactory, serviceName, servicePartition, listenerName)
        {
        }

        public static RegisterSubscriberClient CreateClient(Uri serviceUri = null)
        {
            ClientFactory factory = new ClientFactory();
            ServicePartitionKey servicePartition = new ServicePartitionKey(0);

            if (serviceUri == null)
            {
                return factory.CreateClient<RegisterSubscriberClient, IRegisterSubscriberContract>(microserviceName, servicePartition);
            }
            else
            {
                return factory.CreateClient<RegisterSubscriberClient, IRegisterSubscriberContract>(serviceUri, servicePartition);
            }
        }

        #region IRegisterSubscriberContract
        public Task<HashSet<Topic>> GetAllSubscribedTopics(Uri subcriberUri)
        {
            return MethodWrapperAsync<HashSet<Topic>>("GetAllSubscribedTopics", new object[1] { subcriberUri });
            //return InvokeWithRetryAsync(client => client.Channel.GetAllSubscribedTopics(subcriberUri));
        }

        public Task<bool> SubscribeToTopic(Topic topic, Uri subcriberUri, ServiceType serviceType)
        {
            return MethodWrapperAsync<bool>("SubscribeToTopic", new object[3] { topic, subcriberUri, serviceType });
            //return InvokeWithRetryAsync(client => client.Channel.SubscribeToTopic(topic, subcriberUri, serviceType));
        }

        public Task<bool> SubscribeToTopics(IEnumerable<Topic> topics, Uri subcriberUri, ServiceType serviceType)
        {
            return MethodWrapperAsync<bool>("SubscribeToTopics", new object[3] { topics, subcriberUri, serviceType });
            //return InvokeWithRetryAsync(client => client.Channel.SubscribeToTopics(topics, subcriberUri, serviceType));
        }

        public Task<bool> UnsubscribeFromAllTopics(Uri subcriberUri)
        {
            return MethodWrapperAsync<bool>("UnsubscribeFromAllTopics", new object[1] { subcriberUri });
            //return InvokeWithRetryAsync(client => client.Channel.UnsubscribeFromAllTopics(subcriberUri));
        }

        public Task<bool> UnsubscribeFromTopic(Topic topic, Uri subcriberUri)
        {
            return MethodWrapperAsync<bool>("UnsubscribeFromTopic", new object[2] { topic, subcriberUri });
            //return InvokeWithRetryAsync(client => client.Channel.UnsubscribeFromTopic(topic, subcriberUri));
        }

        public Task<bool> UnsubscribeFromTopics(IEnumerable<Topic> topics, Uri subcriberUri)
        {
            return MethodWrapperAsync<bool>("UnsubscribeFromTopics", new object[2] { topics, subcriberUri });
            //return InvokeWithRetryAsync(client => client.Channel.UnsubscribeFromTopics(topics, subcriberUri));
        }
        #endregion ISubscriberContract
    }
}

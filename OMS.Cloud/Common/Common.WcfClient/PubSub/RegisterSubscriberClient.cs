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
        private static readonly string listenerName = EndpointNames.PubSubRegisterSubscriberEndpoint; 

        public RegisterSubscriberClient(WcfCommunicationClientFactory<IRegisterSubscriberContract> clientFactory, Uri serviceName, ServicePartitionKey servicePartition) 
            : base(clientFactory, serviceName, servicePartition, listenerName)
        {
        }

        public static RegisterSubscriberClient CreateClient()
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<RegisterSubscriberClient, IRegisterSubscriberContract>(microserviceName);
        }

        public static RegisterSubscriberClient CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<RegisterSubscriberClient, IRegisterSubscriberContract>(serviceUri, servicePartitionKey);
        }

        #region IRegisterSubscriberContract
        public Task<HashSet<Topic>> GetAllSubscribedTopics(string subcriberServiceName)
        {
            return MethodWrapperAsync<HashSet<Topic>>("GetAllSubscribedTopics", new object[1] { subcriberServiceName });
            //return InvokeWithRetryAsync(client => client.Channel.GetAllSubscribedTopics(subcriberUri));
        }

        public Task<bool> SubscribeToTopic(Topic topic, string subcriberServiceName)
        {
            return MethodWrapperAsync<bool>("SubscribeToTopic", new object[2] { topic, subcriberServiceName });
            //return InvokeWithRetryAsync(client => client.Channel.SubscribeToTopic(topic, subcriberServiceName));
        }

        public Task<bool> SubscribeToTopics(IEnumerable<Topic> topics, string subcriberServiceName)
        {
            return MethodWrapperAsync<bool>("SubscribeToTopics", new object[2] { topics, subcriberServiceName });
            //return InvokeWithRetryAsync(client => client.Channel.SubscribeToTopics(topics, subcriberServiceName));
        }

        public Task<bool> UnsubscribeFromAllTopics(string subcriberServiceName)
        {
            return MethodWrapperAsync<bool>("UnsubscribeFromAllTopics", new object[1] { subcriberServiceName });
            //return InvokeWithRetryAsync(client => client.Channel.UnsubscribeFromAllTopics(subcriberUri));
        }

        public Task<bool> UnsubscribeFromTopic(Topic topic, string subcriberServiceName)
        {
            return MethodWrapperAsync<bool>("UnsubscribeFromTopic", new object[2] { topic, subcriberServiceName });
            //return InvokeWithRetryAsync(client => client.Channel.UnsubscribeFromTopic(topic, subcriberUri));
        }

        public Task<bool> UnsubscribeFromTopics(IEnumerable<Topic> topics, string subcriberServiceName)
        {
            return MethodWrapperAsync<bool>("UnsubscribeFromTopics", new object[2] { topics, subcriberServiceName });
            //return InvokeWithRetryAsync(client => client.Channel.UnsubscribeFromTopics(topics, subcriberUri));
        }
        #endregion ISubscriberContract
    }
}

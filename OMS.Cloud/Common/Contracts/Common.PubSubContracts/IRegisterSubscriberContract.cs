using Common.CloudContracts;
using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.Cloud;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace OMS.Common.PubSubContracts
{
    [ServiceContract]
    public interface IRegisterSubscriberContract : IService, IHealthChecker
    {
        [OperationContract]
        Task<bool> SubscribeToTopic(Topic topic, string subcriberServiceName);

        [OperationContract]
        Task<bool> SubscribeToTopics(IEnumerable<Topic> topics, string subcriberServiceName);

        [OperationContract]
        Task<HashSet<Topic>> GetAllSubscribedTopics(string subcriberServiceName);

        [OperationContract]
        Task<bool> UnsubscribeFromTopic(Topic topic, string subcriberServiceName);

        [OperationContract]
        Task<bool> UnsubscribeFromTopics(IEnumerable<Topic> topics, string subcriberServiceName);

        [OperationContract]
        Task<bool> UnsubscribeFromAllTopics(string subcriberServiceName);
    }
}

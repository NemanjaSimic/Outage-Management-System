using Microsoft.ServiceFabric.Services.Remoting;
using OMS.Common.Cloud;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace OMS.Common.PubSubContracts
{
    [ServiceContract]
    public interface IRegisterSubscriberContract : IService
    {
        [OperationContract]
        Task<bool> SubscribeToTopic(Topic topic, Uri subcriberUri, ServiceType serviceType);

        [OperationContract]
        Task<bool> SubscribeToTopics(IEnumerable<Topic> topics, Uri subcriberUri, ServiceType serviceType);

        [OperationContract]
        Task<HashSet<Topic>> GetAllSubscribedTopics(Uri subcriberUri);

        [OperationContract]
        Task<bool> UnsubscribeFromTopic(Topic topic, Uri subcriberUri);

        [OperationContract]
        Task<bool> UnsubscribeFromTopics(IEnumerable<Topic> topics, Uri subcriberUri);

        [OperationContract]
        Task<bool> UnsubscribeFromAllTopics(Uri subcriberUri);
    }
}

using Outage.Common.PubSub;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Outage.Common.ServiceContracts.PubSub
{
    [ServiceContract(CallbackContract = typeof(ISubscriberCallback))]
    public interface ISubscriber
    {
        [OperationContract(IsOneWay = true)]
        void Subscribe(Topic topic);
    }

    public interface ISubscriberCallback
    {
        string SubscriberName { get; }

        [OperationContract(IsOneWay = true)]
        void Notify(IPublishableMessage message);
    }
}
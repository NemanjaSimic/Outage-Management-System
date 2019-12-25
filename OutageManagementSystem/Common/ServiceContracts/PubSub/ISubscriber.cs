using Outage.Common.PubSub;
using System.ServiceModel;

namespace Outage.Common.ServiceContracts.PubSub
{
    [ServiceContract(CallbackContract = typeof(IPubSubNotification))]
    public interface ISubscriber
    {
        [OperationContract(IsOneWay = true)]
        void Subscribe(Topic topic);
    }

    public interface IPubSubNotification
    {
        [OperationContract(IsOneWay = true)]
        void Notify(IPublishableMessage message);
    }
}
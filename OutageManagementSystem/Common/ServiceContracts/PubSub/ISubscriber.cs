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

    //[ServiceContract] not needed as IPubSubNotification is never used as contract for ServiceHost
    [ServiceContract]
    public interface IPubSubNotification
    {
        [OperationContract(IsOneWay = true)]
        void Notify(IPublishableMessage message);
    }
}
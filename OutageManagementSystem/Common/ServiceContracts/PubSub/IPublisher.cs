using Outage.Common.PubSub;
using System.ServiceModel;

namespace Outage.Common.ServiceContracts.PubSub
{
    [ServiceContract]
    public interface IPublisher
    {
        [OperationContract]
        void Publish(IPublication publication);
    }
}
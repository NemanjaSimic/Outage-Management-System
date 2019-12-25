using Outage.Common.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.ServiceContracts.PubSub
{
    [ServiceContract(CallbackContract = typeof(INotify))]
    public interface ISubscriber
    {
        [OperationContract]
        void Subscribe(Topic topic);
    }

    //[ServiceContract] not needed as INotify is never used as contract for ServiceHost
    [ServiceContract]
    public interface INotify
    {
        [OperationContract(IsOneWay = true)]
        void Notify(IPublishableMessage message);
    }
}

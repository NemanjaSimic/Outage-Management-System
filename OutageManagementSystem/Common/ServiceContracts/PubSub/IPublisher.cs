using Outage.Common.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.ServiceContracts.PubSub
{
    [ServiceContract]
    public interface IPublisher
    {
        [OperationContract]
        void Publish(IPublication publication);
    }
}

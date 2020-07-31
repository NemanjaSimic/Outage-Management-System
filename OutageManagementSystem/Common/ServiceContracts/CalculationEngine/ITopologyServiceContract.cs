using Microsoft.ServiceFabric.Services.Remoting;
using Outage.Common.UI;
using System.ServiceModel;

namespace Outage.Common.ServiceContracts
{
    [ServiceContract]
    [ServiceKnownType(typeof(UIMeasurement))]
    [ServiceKnownType(typeof(UINode))]
    public interface ITopologyServiceContract
    {
        [OperationContract]
        UIModel GetTopology();
    }
}

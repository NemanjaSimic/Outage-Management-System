using Outage.Common.UI;
using System.ServiceModel;

namespace Outage.Common.ServiceContracts
{
    [ServiceContract]
    public interface ITopologyServiceContract
    {
        [OperationContract]
        UIModel GetTopology();
    }
}

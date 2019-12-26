using CECommon.Model.UI;
using System.ServiceModel;

namespace CECommon.ServiceContracts
{
    [ServiceContract]
    public interface ITopologyServiceContract
    {
        [OperationContract]
        UIModel GetTopology();
    }
}

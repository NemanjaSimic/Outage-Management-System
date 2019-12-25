using CECommon.Model;
using System.ServiceModel;

namespace CECommon.ServiceContracts
{
    [ServiceContract]
    public interface ITopologyServiceContract
    {
        [OperationContract]
        TopologyModel GetTopology();
    }
}

using CECommon;
using System.ServiceModel;

namespace Common.Contracts.WebAdapterContracts
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

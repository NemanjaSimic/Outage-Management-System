using Outage.Common.OutageService.Interface;
using Outage.Common.OutageService.Model;
using System.ServiceModel;

namespace Outage.Common.ServiceContracts.CalculationEngine
{
    [ServiceContract]
    [ServiceKnownType(typeof(OutageTopologyModel))]
    [ServiceKnownType(typeof(OutageTopologyElement))]
    public interface ITopologyOMSService
    {
        [OperationContract]
        IOutageTopologyModel GetOMSModel();
    }
}

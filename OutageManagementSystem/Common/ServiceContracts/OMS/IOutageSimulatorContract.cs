using System.ServiceModel;

namespace Outage.Common.ServiceContracts.OMS
{
    [ServiceContract]
    public interface IOutageSimulatorContract
    {
        [OperationContract]
        bool ResolvedOutage(long outageElementId);

        [OperationContract]
        bool IsOutageElement(long outageElementId);
    }
}

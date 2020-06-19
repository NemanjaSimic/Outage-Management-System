using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;

namespace Outage.Common.ServiceContracts.OMS
{
    [ServiceContract]
    public interface IOutageSimulatorContract
    {
        [OperationContract]
        bool StopOutageSimulation(long outageElementId);

        [OperationContract]
        bool IsOutageElement(long outageElementId);
    }
}

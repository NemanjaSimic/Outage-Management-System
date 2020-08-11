using Common.CloudContracts;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common.OmsContracts.OutageSimulator
{
    [ServiceContract]
    public interface IOutageSimulatorContract : IService, IHealthChecker
    {
        [OperationContract]
        Task<bool> StopOutageSimulation(long outageElementId);

        [OperationContract]
        Task<bool> IsOutageElement(long outageElementId);
    }
}

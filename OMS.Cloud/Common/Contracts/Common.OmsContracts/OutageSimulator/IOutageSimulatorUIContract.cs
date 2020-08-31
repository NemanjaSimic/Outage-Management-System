using Common.CloudContracts;
using Common.OmsContracts.DataContracts.OutageSimulator;
using Microsoft.ServiceFabric.Services.Remoting;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.OmsContracts.OutageSimulator
{
    [ServiceContract]
    public interface IOutageSimulatorUIContract : IService, IHealthChecker
    {
        [OperationContract]
        Task<IEnumerable<SimulatedOutage>> GetAllSimulatedOutages();

        [OperationContract]
        Task<bool> GenerateOutage(SimulatedOutage outage);

        [OperationContract]
        Task<bool> EndOutage(long outageElementGid);
    }
}

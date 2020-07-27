using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common.OmsContracts.OutageSimulator
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

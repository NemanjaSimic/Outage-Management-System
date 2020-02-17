using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

//TODO: sta smo planirali sa ovim?
namespace Outage.Common.ServiceContracts.CalculationEngine
{
    [ServiceContract]
    interface ICECommand
    {
        [OperationContract]
        bool SendAnalogCommand(long gid, float commandingValue);

        [OperationContract]
        bool SendDiscreteCommand(long gid, ushort commandingValue);
    }
}

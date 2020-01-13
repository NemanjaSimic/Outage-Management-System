using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.ServiceContracts.SCADA
{
    [ServiceContract]
    public interface ISCADACommand
    {
        [OperationContract]
        bool SendAnalogCommand(long gid, float commandingValue);

        [OperationContract]
        bool SendDiscreteCommand(long gid, ushort commandingValue);
    }
}

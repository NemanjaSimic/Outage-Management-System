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
        void SendAnalogCommand(long gid, float commandingValue);

        //[OperationContract]
        //void SendAnalogCommand(ushort address, float commandingValue);

        [OperationContract]
        void SendDiscreteCommand(long gid, ushort commandingValue);

        //[OperationContract]
        //void SendDiscreteCommand(ushort address, ushort commandingValue);
    }
}

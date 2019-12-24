using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.ServiceContracts
{
    [ServiceContract]
    public interface ISCADACommand
    {
        [OperationContract]
        void RecvCommand(long gid, object value);
    }
}

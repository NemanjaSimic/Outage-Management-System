using Outage.Common.ServiceContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.ServiceProxies
{
    public class SCADACommandProxy : ClientBase<ISCADACommand>, ISCADACommand
    {
        public void RecvCommand(long gid, object value)
        {
            Channel.RecvCommand(gid, value);
        }
    }
}

using Outage.Common.ServiceContracts;
using System.ServiceModel;

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

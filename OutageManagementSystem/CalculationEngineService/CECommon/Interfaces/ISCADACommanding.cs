using Outage.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon.Interfaces
{
    public interface ISCADACommanding
    {
        void SendAnalogCommand(long gid, float commandingValue, CommandOriginType commandOrigin);

        void SendDiscreteCommand(long guid, int value, CommandOriginType commandOrigin);
       
    }
}

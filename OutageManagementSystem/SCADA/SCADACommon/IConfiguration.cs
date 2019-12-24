using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.SCADA.SCADA_Common
{
    public interface IConfiguration
    {
        int TcpPort { get; set; }
        byte unitAddress { get; set; }
    }
}

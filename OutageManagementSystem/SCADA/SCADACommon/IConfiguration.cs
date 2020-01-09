using System;

namespace Outage.SCADA.SCADACommon
{
    [Obsolete]
    public interface IConfiguration
    {
        int TcpPort { get; set; }
        byte UnitAddress { get; set; }
    }
}
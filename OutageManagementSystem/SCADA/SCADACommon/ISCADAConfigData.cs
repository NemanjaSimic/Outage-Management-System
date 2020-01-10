using System;

namespace Outage.SCADA.SCADACommon
{
    public interface ISCADAConfigData
    {
        ushort TcpPort { get; }
        string IpAddress { get; }
        byte UnitAddress { get; }
        ushort Interval { get; }
        string MdbSimExeName { get; }
        string MdbSimExePath { get; }
    }
}
using System.Net;

namespace OMS.Cloud.SCADA.Common
{
    public interface ISCADAConfigData
    {
        ushort TcpPort { get; }
        IPAddress IpAddress { get; }
        byte UnitAddress { get; }
        ushort Interval { get; }
        string ModbusSimulatorExeName { get; }
        string ModbusSimulatorExePath { get; }
    }
}
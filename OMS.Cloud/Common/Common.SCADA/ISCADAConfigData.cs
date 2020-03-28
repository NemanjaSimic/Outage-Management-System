using System.Net;

namespace OMS.Common.SCADA
{
    public interface ISCADAConfigData
    {
        ushort TcpPort { get; }
        IPAddress IpAddress { get; }
        byte UnitAddress { get; }
        ushort AcquisitionInterval { get; }
        ushort FunctionExecutionInterval { get; }
        string ModbusSimulatorExeName { get; }
        string ModbusSimulatorExePath { get; }
    }
}
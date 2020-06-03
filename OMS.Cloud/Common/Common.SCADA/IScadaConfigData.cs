using System.Net;

namespace OMS.Common.SCADA
{
    public interface IScadaConfigData
    {
        ushort AcquisitionInterval { get; }
        ushort FunctionExecutionInterval { get; }
        IPAddress IpAddress { get; }
        string ModbusSimulatorExeName { get; }
        string ModbusSimulatorExePath { get; }
        ushort TcpPort { get; }
        byte UnitAddress { get; }
    }
}
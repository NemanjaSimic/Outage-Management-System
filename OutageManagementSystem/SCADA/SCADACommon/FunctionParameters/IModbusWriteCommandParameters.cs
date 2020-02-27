namespace Outage.SCADA.SCADACommon.FunctionParameters
{
    public interface IModbusWriteCommandParameters : IModbusCommandParameters
    {
        ushort OutputAddress { get; }
        int Value { get; }
    }
}

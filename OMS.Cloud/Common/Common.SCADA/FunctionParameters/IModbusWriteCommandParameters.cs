namespace OMS.Common.SCADA.FunctionParameters
{
    public interface IModbusWriteCommandParameters : IModbusCommandParameters
    {
        ushort OutputAddress { get; }
        int Value { get; }
    }
}

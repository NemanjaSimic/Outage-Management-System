namespace OMS.Cloud.SCADA.Common.FunctionParameters
{
    public interface IModbusWriteCommandParameters : IModbusCommandParameters
    {
        ushort OutputAddress { get; }
        int Value { get; }
    }
}

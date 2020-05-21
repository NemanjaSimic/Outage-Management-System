using Outage.Common;

namespace OMS.Common.SCADA
{
    public interface IModbusFunction
    {
        ModbusFunctionCode FunctionCode { get; }
    }

    public interface IWriteModbusFunction : IModbusFunction
    {
        CommandOriginType CommandOrigin { get; }
    }

    public interface IWriteSingleFunction : IWriteModbusFunction
    {
        ushort OutputAddress { get; }
        int CommandValue { get; }
    }

    public interface IWriteMultipleFunction : IWriteModbusFunction
    {
        ushort StartAddress { get; }
        int[] CommandValues { get; }
    }

    public interface IReadModbusFunction : IModbusFunction
    {
        ushort StartAddress { get; }
        ushort Quantity { get; }
    }
}
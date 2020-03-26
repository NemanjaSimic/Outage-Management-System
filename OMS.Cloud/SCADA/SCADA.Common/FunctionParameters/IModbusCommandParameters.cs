namespace OMS.Cloud.SCADA.Common.FunctionParameters
{
    public interface IModbusCommandParameters
    {
        byte FunctionCode { get; }
        ushort Length { get; }
        ushort ProtocolId { get; }
        ushort TransactionId { get; }
        byte UnitId { get; }
    }
}
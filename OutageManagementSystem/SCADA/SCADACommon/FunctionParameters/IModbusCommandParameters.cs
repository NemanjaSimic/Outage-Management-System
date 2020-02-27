namespace Outage.SCADA.SCADACommon.FunctionParameters
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
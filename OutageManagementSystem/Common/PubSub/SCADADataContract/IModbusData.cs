namespace Outage.Common.PubSub.SCADADataContract
{
    public interface IModbusData
    {
        long MeasurementGid { get; }
        AlarmType Alarm { get; }
        CommandOriginType CommandOrigin { get; }
    }
}

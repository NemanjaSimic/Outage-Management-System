using OMS.Common.Cloud;

namespace OMS.Common.PubSub
{
    public interface IModbusData
    {
        long MeasurementGid { get; }
        AlarmType Alarm { get; }
        CommandOriginType CommandOrigin { get; }
    }
}

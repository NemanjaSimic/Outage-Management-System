using OMS.Common.Cloud;

namespace OMS.Common.PubSubContracts.Interfaces
{
    public interface IModbusData
    {
        long MeasurementGid { get; }
        AlarmType Alarm { get; }
        CommandOriginType CommandOrigin { get; }
    }
}

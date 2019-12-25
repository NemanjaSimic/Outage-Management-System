using System;

namespace Outage.SCADA.SCADA_Common
{
    public interface IPoint
    {
        int PointId { get; }

        ushort RawValue { get; set; }

        AlarmType Alarm { get; set; }

        IConfigItem ConfigItem { get; }

        DateTime Timestamp { get; set; }
    }
}
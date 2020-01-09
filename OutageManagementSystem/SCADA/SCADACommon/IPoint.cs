using System;

namespace Outage.SCADA.SCADACommon
{
    [Obsolete]
    public interface IPoint
    {
        int PointId { get; }

        ushort RawValue { get; set; }

        AlarmType Alarm { get; set; }

        IModbusPoint ConfigItem { get; }

        DateTime Timestamp { get; set; }
    }
}
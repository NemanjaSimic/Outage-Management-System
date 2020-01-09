using System;

namespace Outage.SCADA.SCADACommon
{
    [Obsolete]
    public interface IResponse
    {
        long GID { get; set; }
        AlarmType Alarm { get; set; }
        object Value { get; set; }
    }
}
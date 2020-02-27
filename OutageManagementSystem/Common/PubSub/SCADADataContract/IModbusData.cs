using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.Common.PubSub.SCADADataContract
{
    public interface IModbusData
    {
        long MeasurementGid { get; }
        AlarmType Alarm { get; }
        CommandOriginType CommandOrigin { get; }
    }
}

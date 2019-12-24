using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

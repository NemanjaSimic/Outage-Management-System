using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.SCADA.SCADA_Common
{
    public interface IResponse
    {

        long GID { get; set; }
        AlarmType Alarm { get; set; }
        object Value { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.SCADA.SCADA_Common
{
    public interface IConfigItem
    {
        string Name { get; set; }
        long Gid { get; set; }
        PointType RegistarType { get; set; }
        ushort Address { get; set; }
        float MinValue { get; set; }
        float MaxValue { get; set; }
        float DefaultValue { get; set; }
        float CurrentValue { get; set; }
        double ScaleFactor { get; set; }
        double Deviation { get; set; }
        double EGU_Min { get; set; }
        double EGU_Max { get; set; }
        ushort AbnormalValue { get; set; }
        double HighLimit { get; set; }

        double LowLimit { get; set; }

    }
}

using CECommon.Interfaces;
using Outage.Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon
{
    public class SmartCache
    {
        public SmartCache()
        {
           
        }
    }

    struct AnalogMeasurementInforamtion
    {
        public long elementGid;
        public float value;
        public AnalogMeasurementInforamtion(long elementGid, float value)
        {
            this.elementGid = elementGid;
            this.value = value;
        }

    }
    struct DiscreteMeasurementInforamtion
    {
        public long elementGid;
        public ushort value;
        public DiscreteMeasurementInforamtion(long elementGid, ushort value)
        {
            this.elementGid = elementGid;
            this.value = value;
        }

    }
}

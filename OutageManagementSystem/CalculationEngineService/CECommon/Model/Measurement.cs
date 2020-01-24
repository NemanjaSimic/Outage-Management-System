using CECommon.Interfaces;
using Outage.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CECommon.Model
{
    public abstract class Measurement : IMeasurement
    {
        public long Id { get; set; }
        public string Address { get; set; }
        public bool isInput { get; set; }
    }

    public class DiscreteMeasurement : Measurement
    {
        public bool CurrentOpen { get; set; }
        public DiscreteMeasurementType MeasurementType { get; set; }
        public int MaxValue { get; set; }
        public int MinValue { get; set; }
        public int NormalValue { get; set; }

    }

    public class AnalogMeasurement : Measurement
    {
        public float CurrentValue { get; set; }
        public float MaxValue { get; set; }
        public float MinValue { get; set; }
        public float NormalValue { get; set; }
        public float Deviation { get; set; }
        public float ScalingFactor { get; set; }
        public AnalogMeasurementType SignalType { get; set;}


    }
}

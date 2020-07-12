using CECommon.Interfaces;
using OMS.Common.Cloud;

namespace CECommon.Model
{
    public abstract class Measurement : IMeasurement
    {
        public long Id { get; set; }
        public long ElementId { get; set; }
        public string Address { get; set; }
        public bool IsInput { get; set; }
        public AlarmType Alarm { get; set; }
        public abstract string GetMeasurementType();
        public abstract float GetCurrentValue();

    }

    public class DiscreteMeasurement : Measurement
    {
        public bool CurrentOpen { get; set; }
        public DiscreteMeasurementType MeasurementType { get; set; }
        public int MaxValue { get; set; }
        public int MinValue { get; set; }
        public int NormalValue { get; set; }
        public override string GetMeasurementType() => MeasurementType.ToString();
        public override float GetCurrentValue() => (CurrentOpen) ? 1 : 0;
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
        public override string GetMeasurementType() => SignalType.ToString();
        public override float GetCurrentValue() => CurrentValue;
    }

    public class ArtificalDiscreteMeasurement : DiscreteMeasurement
    {
       
    }
}

using OMS.Common.Cloud;

namespace Common.CE.Interfaces
{
	public interface IAnalogMeasurement : IMeasurement
	{
        float CurrentValue { get; set; }
        float MaxValue { get; set; }
        float MinValue { get; set; }
        float NormalValue { get; set; }
        float Deviation { get; set; }
        float ScalingFactor { get; set; }
        AnalogMeasurementType SignalType { get; set; }
    }
}

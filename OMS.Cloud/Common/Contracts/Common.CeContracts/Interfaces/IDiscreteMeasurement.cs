using OMS.Common.Cloud;

namespace Common.CeContracts
{
	public interface IDiscreteMeasurement : IMeasurement
	{
		bool CurrentOpen { get; set; }
		int MaxValue { get; set; }
		DiscreteMeasurementType MeasurementType { get; set; }
		int MinValue { get; set; }
		int NormalValue { get; set; }
	}
}
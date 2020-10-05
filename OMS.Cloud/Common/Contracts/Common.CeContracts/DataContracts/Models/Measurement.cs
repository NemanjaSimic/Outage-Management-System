using OMS.Common.Cloud;
using System.Runtime.Serialization;

namespace Common.CeContracts
{
	[DataContract]
    [KnownType(typeof(ArtificalDiscreteMeasurement))]
    [KnownType(typeof(DiscreteMeasurement))]
    [KnownType(typeof(AnalogMeasurement))]

    public abstract class Measurement //: IMeasurement
    {
		[DataMember]
        public long Id { get; set; }
        [DataMember]
        public long ElementId { get; set; }
        [DataMember]
        public string Address { get; set; }
        [DataMember]
        public bool IsInput { get; set; }
        [DataMember]
        public AlarmType Alarm { get; set; }
        public abstract string GetMeasurementType();
        public abstract float GetCurrentValue();
    }

	[DataContract]
    [KnownType(typeof(ArtificalDiscreteMeasurement))]
	public class DiscreteMeasurement : Measurement//, IDiscreteMeasurement
	{
		[DataMember]
		public bool CurrentOpen { get; set; }
		[DataMember]
		public DiscreteMeasurementType MeasurementType { get; set; }
		[DataMember]
		public int MaxValue { get; set; }
		[DataMember]
		public int MinValue { get; set; }
		[DataMember]
		public int NormalValue { get; set; }
		public override string GetMeasurementType() => MeasurementType.ToString();
		public override float GetCurrentValue() => (CurrentOpen) ? 1 : 0;

        public DiscreteMeasurement()
        {
            Alarm = AlarmType.NO_ALARM;
        }
	}

	[DataContract]
    public class AnalogMeasurement : Measurement//, IAnalogMeasurement
    {
		[DataMember]
        public float CurrentValue { get; set; }
        [DataMember]
        public float MaxValue { get; set; }
        [DataMember]
        public float MinValue { get; set; }
        [DataMember]
        public float NormalValue { get; set; }
        [DataMember]
        public float Deviation { get; set; }
        [DataMember]
        public float ScalingFactor { get; set; }
		[DataMember]
        public AnalogMeasurementType SignalType { get; set; }
        public override string GetMeasurementType() => SignalType.ToString();
        public override float GetCurrentValue() => CurrentValue;

        public AnalogMeasurement()
        {
            Alarm = AlarmType.NO_ALARM;
        }
    }

    [DataContract]
    public class ArtificalDiscreteMeasurement : DiscreteMeasurement
    {
        public ArtificalDiscreteMeasurement()
        {
            Alarm = AlarmType.NO_ALARM;
        }
    }
}

using Outage.Common;
using System;
using System.Runtime.Serialization;

namespace OMS.Common.ScadaContracts.DataContracts
{
    [DataContract]
    public class AnalogPointItem : ScadaModelPointItem, IAnalogPointItem
    {
        private float currentEguValue;

        public AnalogPointItem(ISetAlarmStrategy alarmStrategy)
            : base(alarmStrategy)
        {
        }

        [DataMember]
        public float NormalValue { get; set; }
        [DataMember]
        public float CurrentEguValue
        {
            get { return currentEguValue; }
            set
            {
                currentEguValue = value;
                SetAlarms();
            }
        }
        
        [DataMember]
        public float EGU_Min { get; set; }
        [DataMember]
        public float EGU_Max { get; set; }
        [DataMember]
        public float ScalingFactor { get; set; }
        [DataMember]
        public float Deviation { get; set; }
        [DataMember]
        public AnalogMeasurementType AnalogType { get; set; }
        
        [DataMember]
        public int CurrentRawValue
        {
            get
            {
                return EguToRawValueConversion(CurrentEguValue);
            }
        }

        [DataMember]
        public int MinRawValue
        {
            get
            {
                return EguToRawValueConversion(CurrentEguValue);
            }
        }

        [DataMember]
        public int MaxRawValue
        {
            get
            {
                return EguToRawValueConversion(CurrentEguValue);
            }
        }

        public override bool SetAlarms()
        {
            return alarmStrategy.SetAlarm(this);
        }

        #region Conversions

        public float RawToEguValueConversion(int rawValue)
        {
            float eguValue = ((ScalingFactor * rawValue) + Deviation);

            if (eguValue > float.MaxValue || eguValue < float.MinValue)
            {
                throw new Exception($"Egu value: {eguValue} is out of float data type boundaries [{float.MinValue}, {float.MaxValue}]");
            }

            return eguValue;
        }

        public int EguToRawValueConversion(float eguValue)
        {
            if (ScalingFactor == 0)
            {
                throw new DivideByZeroException($"Scaling factor is zero.");
            }

            int rawValue = (int)((eguValue - Deviation) / ScalingFactor);

            return rawValue;
        }

        #endregion

        #region IClonable

        public override IScadaModelPointItem Clone()
        {
            return this.MemberwiseClone() as IScadaModelPointItem;
        }

        #endregion IClonable
    }
}

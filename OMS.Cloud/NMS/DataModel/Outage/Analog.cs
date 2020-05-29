using Outage.Common;
using OMS.Common.NmsContracts.GDA;

namespace NMS.DataModel
{
    public class Analog : Measurement
    {
        #region Fields
        private float currentValue;
        private float maxValue;
        private float minValue;
        private float normalValue;
        private float scalingFactor;
        private float deviation;
        private AnalogMeasurementType signalType;
        #endregion

        public Analog(long globalId) : base(globalId)
        {
        }

        protected Analog(Analog analog) : base(analog)
        {
            CurrentValue = analog.CurrentValue;
            MaxValue = analog.MaxValue;
            MinValue = analog.MinValue;
            NormalValue = analog.NormalValue;
            SignalType = analog.SignalType;
            ScalingFactor = analog.ScalingFactor;
            Deviation = analog.Deviation;
        }

        #region Properties
        public float CurrentValue
        {
            get { return currentValue; }
            set { currentValue = value; }
        }

        public float MaxValue
        {
            get { return maxValue; }
            set { maxValue = value; }
        }

        public float MinValue
        {
            get { return minValue; }
            set { minValue = value; }
        }

        public float NormalValue
        {
            get { return normalValue; }
            set { normalValue = value; }
        }

        public AnalogMeasurementType SignalType
        {
            get { return signalType; }
            set { signalType = value; }
        }

        public float ScalingFactor
        {
            get { return scalingFactor; }
            set { scalingFactor = value; }
        }

        public float Deviation
        {
            get { return deviation; }
            set { deviation = value; }
        }
        #endregion

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                Analog x = (Analog)obj;
                return (x.currentValue == this.currentValue &&
                        x.maxValue == this.maxValue &&
                        x.minValue == this.minValue &&
                        x.normalValue == this.normalValue &&
                        x.signalType == this.signalType) &&
                        x.scalingFactor == this.scalingFactor &&
                        x.deviation == this.deviation;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #region IAccsess implementation
        public override bool HasProperty(ModelCode property)
        {
            switch (property)
            {
                case ModelCode.ANALOG_CURRENTVALUE:
                case ModelCode.ANALOG_MAXVALUE:
                case ModelCode.ANALOG_MINVALUE:
                case ModelCode.ANALOG_NORMALVALUE:
                case ModelCode.ANALOG_SIGNALTYPE:
                case ModelCode.ANALOG_SCALINGFACTOR:
                case ModelCode.ANALOG_DEVIATION:
                    return true;
                default:
                    return base.HasProperty(property);
            }
        }

        public override void GetProperty(Property property)
        {
            switch (property.Id)
            {
                case ModelCode.ANALOG_CURRENTVALUE:
                    property.SetValue(currentValue);
                    break;
                case ModelCode.ANALOG_MAXVALUE:
                    property.SetValue(maxValue);
                    break;
                case ModelCode.ANALOG_MINVALUE:
                    property.SetValue(minValue);
                    break;
                case ModelCode.ANALOG_NORMALVALUE:
                    property.SetValue(normalValue);
                    break;
                case ModelCode.ANALOG_SIGNALTYPE:
                    property.SetValue((short)signalType);
                    break;
                case ModelCode.ANALOG_SCALINGFACTOR:
                    property.SetValue(scalingFactor);
                    break;
                case ModelCode.ANALOG_DEVIATION:
                    property.SetValue(deviation);
                    break;
                default:
                    base.GetProperty(property);
                    break;
            }
        }

        public override void SetProperty(Property property)
        {
            switch (property.Id)
            {
                case ModelCode.ANALOG_CURRENTVALUE:
                    currentValue = property.AsFloat();
                    break;
                case ModelCode.ANALOG_MAXVALUE:
                    maxValue = property.AsFloat();
                    break;
                case ModelCode.ANALOG_MINVALUE:
                    minValue = property.AsFloat();
                    break;
                case ModelCode.ANALOG_NORMALVALUE:
                    normalValue = property.AsFloat();
                    break;
                case ModelCode.ANALOG_SIGNALTYPE:
                    signalType = (AnalogMeasurementType)property.AsEnum();
                    break;
                case ModelCode.ANALOG_SCALINGFACTOR:
                    scalingFactor = property.AsFloat();
                    break;
                case ModelCode.ANALOG_DEVIATION:
                    deviation = property.AsFloat();
                    break;
                default:
                    base.SetProperty(property);
                    break;
            }
        }
        #endregion

        #region IClonable
        public override IdentifiedObject Clone()
        {
            return new Analog(this);
        }
        #endregion
    }
}

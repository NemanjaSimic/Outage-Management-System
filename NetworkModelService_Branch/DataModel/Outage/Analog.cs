using Outage.Common;
using Outage.Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.DataModel
{
    public class Analog : Measurement
    {
        private float currentValue;
        private float maxValue;
        private float minValue;
        private float normalValue;
        private AnalogMeasurementType signalType;



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

        public Analog(long globalId) : base(globalId)
        {
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                Analog x = (Analog)obj;
                return (x.currentValue == this.currentValue &&
                        x.maxValue == this.maxValue &&
                        x.minValue == this.minValue &&
                        x.normalValue == this.normalValue &&
                        x.signalType == this.signalType);
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
                default:
                    base.SetProperty(property);
                    break;
            }
        }
        #endregion
    }
}

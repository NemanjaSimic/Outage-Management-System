using Outage.Common;
using OMS.Common.NmsContracts.GDA;

namespace OMS.Cloud.NMS.DataModel
{
    public class Discrete : Measurement
    {
        #region Fields
        private bool currentOpen;
        private int maxValue;
        private int minValue;
        private int normalValue;
        private DiscreteMeasurementType measurementType;
        #endregion

        public Discrete(long globalId) : base(globalId)
        {
        }

        protected Discrete(Discrete discrete) : base(discrete)
        {
            CurrentOpen = discrete.CurrentOpen;
            MaxValue = discrete.MaxValue;
            MinValue = discrete.MinValue;
            NormalValue = discrete.NormalValue;
            MeasurementType = discrete.MeasurementType;
        }

        #region Properties
        public bool CurrentOpen
        {
            get { return currentOpen; }
            set { currentOpen = value; }
        }

        public int MaxValue
        {
            get { return maxValue; }
            set { maxValue = value; }
        }

        public DiscreteMeasurementType MeasurementType
        {
            get { return measurementType; }
            set { measurementType = value; }
        }

        public int MinValue
        {
            get { return minValue; }
            set { minValue = value; }
        }       

        public int NormalValue
        {
            get { return normalValue; }
            set { normalValue = value; }
        }
        #endregion Properties

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                Discrete x = (Discrete)obj;
                return (x.currentOpen == this.currentOpen &&
                        x.maxValue == this.maxValue &&
                        x.measurementType == this.measurementType &&
                        x.minValue == this.minValue &&
                        x.normalValue == this.minValue);
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

        #region IAccess implementation
        public override bool HasProperty(ModelCode property)
        {
            switch (property)
            {
                case ModelCode.DISCRETE_CURRENTOPEN:
                case ModelCode.DISCRETE_MAXVALUE:
                case ModelCode.DISCRETE_MEASUREMENTTYPE:
                case ModelCode.DISCRETE_MINVALUE:
                case ModelCode.DISCRETE_NORMALVALUE:
                    return true;
                default:
                    return base.HasProperty(property);
            }
        }

        public override void GetProperty(Property property)
        {
            switch (property.Id)
            {
                case ModelCode.DISCRETE_CURRENTOPEN:
                    property.SetValue(currentOpen);
                    break;
                case ModelCode.DISCRETE_MAXVALUE:
                    property.SetValue(maxValue);
                    break;
                case ModelCode.DISCRETE_MEASUREMENTTYPE:
                    property.SetValue((short)measurementType);
                    break;
                case ModelCode.DISCRETE_MINVALUE:
                    property.SetValue(minValue);
                    break;
                case ModelCode.DISCRETE_NORMALVALUE:
                    property.SetValue(normalValue);
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
                case ModelCode.DISCRETE_CURRENTOPEN:
                    currentOpen = property.AsBool();
                    break;
                case ModelCode.DISCRETE_MAXVALUE:
                    maxValue = property.AsInt();
                    break;
                case ModelCode.DISCRETE_MEASUREMENTTYPE:
                    measurementType = (DiscreteMeasurementType)property.AsEnum();
                    break;
                case ModelCode.DISCRETE_MINVALUE:
                    minValue = property.AsInt();
                    break;
                case ModelCode.DISCRETE_NORMALVALUE:
                    normalValue = property.AsInt();
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
            return new Discrete(this);
        }
        #endregion
    }
}

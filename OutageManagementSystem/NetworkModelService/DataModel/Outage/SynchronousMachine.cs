using Outage.Common;
using Outage.Common.GDA;

namespace Outage.DataModel
{
    public class SynchronousMachine : ConductingEquipment
    {
        #region Fields
        private float capacity;
        private float currentRegime;
		#endregion

		public SynchronousMachine(long globalId) : base(globalId)
        {
        }

        public SynchronousMachine(SynchronousMachine sm) : base (sm)
        {
            Capacity = sm.Capacity;
            CurrentRegime = sm.CurrentRegime;
        }

		#region Properties
        public float Capacity
        {
            get { return capacity;}
            set { capacity = value; }
        }

        public float CurrentRegime
        {
            get { return currentRegime; }
            set { currentRegime = value; }
        }
        #endregion

        public override bool Equals(object obj)
        {
            if(base.Equals(obj))
            {
                SynchronousMachine x = (SynchronousMachine)obj;
                return (x.CurrentRegime == this.CurrentRegime && x.Capacity == this.Capacity);
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
                case ModelCode.SYNCHRONOUSMACHINE_CURRENTREGIME:
                    return true;
                case ModelCode.SYNCHRONOUSMACHINE_CAPACITY:
                    return true;
                default:
                    return base.HasProperty(property);
            }
        }

        public override void GetProperty(Property property)
        {
            switch (property.Id)
            {
                case ModelCode.SYNCHRONOUSMACHINE_CURRENTREGIME:
                    property.SetValue(currentRegime);
                    break;
                case ModelCode.SYNCHRONOUSMACHINE_CAPACITY:
                    property.SetValue(capacity);
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
                case ModelCode.SYNCHRONOUSMACHINE_CURRENTREGIME:
                    currentRegime = property.AsFloat();
                    break;
                case ModelCode.SYNCHRONOUSMACHINE_CAPACITY:
                    capacity = property.AsFloat();
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
            return new SynchronousMachine(this);
        }
        #endregion
    }
}

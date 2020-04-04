using Outage.Common;
using OMS.Common.NmsContracts.GDA;

namespace OMS.Cloud.NMS.DataModel
{
    public class Breaker : ProtectedSwitch
    {
        #region Fields
        private bool noReclosing;
        #endregion

        public Breaker(long globalId) : base(globalId)
        {
        }

        protected Breaker(Breaker br) : base(br)
        {
            NoReclosing = br.NoReclosing;
        }

        #region Properties
        public bool NoReclosing
        {
            get { return noReclosing; }
            set { noReclosing = value; }
        }
        #endregion

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                Breaker x = (Breaker)obj;
                return x.noReclosing == this.noReclosing;
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
                case ModelCode.BREAKER_NORECLOSING:
                    return true;
                default:
                    return base.HasProperty(property);
            }
        }

        public override void GetProperty(Property property)
        {
            switch (property.Id)
            {
                case ModelCode.BREAKER_NORECLOSING:
                    property.SetValue(noReclosing);
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
                case ModelCode.BREAKER_NORECLOSING:
                    noReclosing = property.AsBool();
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
            return new Breaker(this);
        }
        #endregion
    }
}

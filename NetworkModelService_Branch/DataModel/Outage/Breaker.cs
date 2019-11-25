using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Outage.Common;
using Outage.Common.GDA;

namespace Outage.DataModel
{
    public class Breaker : ProtectedSwitch
    {

        private bool noReclosing;

        public bool NoReclosing
        {
            get { return noReclosing; }
            set { noReclosing = value; }
        }

        public Breaker(long globalId) : base(globalId)
        {
        }

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
    }
}

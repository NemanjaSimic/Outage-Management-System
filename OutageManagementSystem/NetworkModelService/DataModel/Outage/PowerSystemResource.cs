using Outage.Common;
using Outage.Common.GDA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Outage.DataModel
{
	public class PowerSystemResource : IdentifiedObject
	{
		public PowerSystemResource(long globalId) : base(globalId)
		{
		}	

		protected PowerSystemResource(PowerSystemResource psr) : base(psr)
        {
        } 

		public override bool Equals(object obj)
		{

            return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}		

		#region IAccess implementation

		public override bool HasProperty(ModelCode property)
		{
            return base.HasProperty(property);
		}

		public override void GetProperty(Property property)
		{
            base.GetProperty(property);
		}

		public override void SetProperty(Property property)
		{
            base.SetProperty(property);
		}

        #endregion IAccess implementation

        #region IClonable
        public override IdentifiedObject Clone()
        {
            return new PowerSystemResource(this);
        }
        #endregion
    }
}

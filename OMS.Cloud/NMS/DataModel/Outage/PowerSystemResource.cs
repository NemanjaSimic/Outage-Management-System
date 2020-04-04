using Outage.Common;
using OMS.Common.NmsContracts.GDA;

namespace OMS.Cloud.NMS.DataModel
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

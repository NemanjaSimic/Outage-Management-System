using Outage.Common;
using OMS.Common.NmsContracts.GDA;

namespace OMS.Cloud.NMS.DataModel
{
    public class ACLineSegment : Conductor
    {
        public ACLineSegment(long globalId) : base(globalId)
        {
        }

        protected ACLineSegment(ACLineSegment aclSeg) : base(aclSeg)
        {
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj)) //in case we add new props
            {
                return true;
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
            switch (property) //in case we add new props
            {
                //case ModelCode.EQUIPMENT_AGGREGATE:
                //case ModelCode.EQUIPMENT_NORMALLYINSERVICE:

                //	return true;
                default:
                    return base.HasProperty(property);
            }
        }

        public override void GetProperty(Property property)
        {
            switch (property.Id) //in case we add new props
            {
                //case ModelCode.EQUIPMENT_AGGREGATE:
                //	property.SetValue(aggregate);
                //	break;

                //case ModelCode.EQUIPMENT_NORMALLYINSERVICE:
                //	property.SetValue(normallyInService);
                //	break;

                default:
                    base.GetProperty(property);
                    break;
            }
        }

        public override void SetProperty(Property property)
        {
            switch (property.Id) //in case we add new props
            {
                //case ModelCode.EQUIPMENT_AGGREGATE:
                //	aggregate = property.AsBool();
                //	break;

                //case ModelCode.EQUIPMENT_NORMALLYINSERVICE:
                //	normallyInService = property.AsBool();
                //	break;

                default:
                    base.SetProperty(property);
                    break;
            }
        }

        #endregion IAccess implementation

        #region IClonable

        public override IdentifiedObject Clone()
        {
            return new ACLineSegment(this);
        }

        #endregion IClonable
    }
}
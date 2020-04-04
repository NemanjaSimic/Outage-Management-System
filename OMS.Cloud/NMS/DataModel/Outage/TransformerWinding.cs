using System.Collections.Generic;
using OMS.Common.NmsContracts.GDA;
using Outage.Common;

namespace OMS.Cloud.NMS.DataModel
{
    public class TransformerWinding : ConductingEquipment
    {
        #region Fields
        private long powerTransformer;
        #endregion

        public TransformerWinding(long globalId) : base(globalId)
        {
        }

        protected TransformerWinding(TransformerWinding tw) : base(tw)
        {
            PowerTransformer = tw.PowerTransformer;
        }

        #region Properties
        public long PowerTransformer
        {
            get { return powerTransformer; }
            set { powerTransformer = value; }
        }
        #endregion

        public override bool Equals(object obj)
        {
            if(base.Equals(obj))
            {
                TransformerWinding x = (TransformerWinding)obj;
                return x.PowerTransformer == this.PowerTransformer;
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
                case ModelCode.TRANSFORMERWINDING_POWERTRANSFORMER:
                    return true;
                default:
                    return base.HasProperty(property);
            }
        }

        public override void GetProperty(Property prop)
        {
            switch (prop.Id)
            {
                case ModelCode.TRANSFORMERWINDING_POWERTRANSFORMER:
                    prop.SetValue(PowerTransformer);
                    break;
                default:
                    base.GetProperty(prop);
                    break;
            }
        }

        public override void SetProperty(Property property)
        {
            switch (property.Id)
            {
                case ModelCode.TRANSFORMERWINDING_POWERTRANSFORMER:
                    powerTransformer = property.AsReference();
                    break;
                default:
                    base.SetProperty(property);
                    break;
            }
        }
        #endregion

        #region IReference implementation
        public override void GetReferences(Dictionary<ModelCode, List<long>> references, TypeOfReference refType)
        {
            if (powerTransformer != 0 && (refType == TypeOfReference.Reference || refType == TypeOfReference.Both))
            {
                references[ModelCode.TRANSFORMERWINDING_POWERTRANSFORMER] = new List<long>();
                references[ModelCode.TRANSFORMERWINDING_POWERTRANSFORMER].Add(powerTransformer);
            }
            base.GetReferences(references, refType);
        }
        #endregion

        #region IClonable
        public override IdentifiedObject Clone()
        {
            return new TransformerWinding(this);
        }
        #endregion
    }
}

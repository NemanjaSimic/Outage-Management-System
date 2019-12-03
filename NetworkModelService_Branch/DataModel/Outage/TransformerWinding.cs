using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Outage.Common;
using Outage.Common.GDA;

namespace Outage.DataModel
{
    public class TransformerWinding : ConductingEquipment
    {
        private long powerTransformer;

        public long PowerTransformer
        {
            get { return powerTransformer; }
            set { powerTransformer = value; }
        }

        public TransformerWinding(long globalId) : base(globalId)
        {
        }

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
    }
}

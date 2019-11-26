using Outage.Common;
using Outage.Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.DataModel
{
    public class PowerTransformer : Equipment
    {
        private List<long> transformerWindings = new List<long>();

        public List<long> TransformerWindings
        {
            get { return transformerWindings; }
            set { transformerWindings = value; }
        }


        public PowerTransformer(long globalId) : base(globalId)
        {
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                PowerTransformer x = (PowerTransformer)obj;
                return CompareHelper.CompareLists(x.transformerWindings, this.transformerWindings);
            }
            else
            {
                return false;
            }
        }

        #region IAccess implementation

        public override bool HasProperty(ModelCode property)
        {
            switch (property)
            {
                case ModelCode.POWERTRANSFORMER_TRANSFORMERWINDINGS:
                    return true;
                default:
                    return base.HasProperty(property);
            }
        }

        public override void GetProperty(Property property)
        {
            switch (property.Id)
            {
                case ModelCode.POWERTRANSFORMER_TRANSFORMERWINDINGS:
                    property.SetValue(transformerWindings);
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
                default:
                    base.SetProperty(property);
                    break;
            }
        }
        #endregion

        #region
        public override bool IsReferenced
        {
            get
            {
                return transformerWindings.Count > 0 || base.IsReferenced;
            }
        }

        public override void GetReferences(Dictionary<ModelCode, List<long>> references, TypeOfReference refType)
        {
            if (transformerWindings != null && transformerWindings.Count > 0 && (refType == TypeOfReference.Target || refType == TypeOfReference.Both))
            {
                references[ModelCode.POWERTRANSFORMER_TRANSFORMERWINDINGS] = transformerWindings.GetRange(0, transformerWindings.Count);
            }
            base.GetReferences(references, refType);
        }

        public override void AddReference(ModelCode referenceId, long globalId)
        {
            switch (referenceId)
            {
                case ModelCode.TRANSFORMERWINDING_POWERTRANSFORMER:
                    transformerWindings.Add(globalId);
                    break;
                default:
                    base.AddReference(referenceId, globalId);
                    break;
            }
        }

        public override void RemoveReference(ModelCode referenceId, long globalId)
        {
            switch (referenceId)
            {
                case ModelCode.TRANSFORMERWINDING_POWERTRANSFORMER:

                    if (transformerWindings.Contains(globalId))
                    {
                        transformerWindings.Remove(globalId);
                    }
                    else
                    {
                        CommonTrace.WriteTrace(CommonTrace.TraceWarning, "Entity (GID = 0x{0:x16}) doesn't contain reference 0x{1:x16}.", this.GlobalId, globalId);
                    }

                    break;

                default:
                    base.RemoveReference(referenceId, globalId);
                    break;
            }
        }
        #endregion

    }
}

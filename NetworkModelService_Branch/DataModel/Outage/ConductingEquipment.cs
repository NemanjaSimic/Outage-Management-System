using Outage.Common;
using Outage.Common.GDA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;


namespace Outage.DataModel
{
	public class ConductingEquipment : Equipment
	{
        #region Fields
        private long baseVoltage;
        private List<long> terminals = new List<long>();
        #endregion

        public ConductingEquipment(long globalId) : base(globalId) 
		{
		}

        protected ConductingEquipment(ConductingEquipment ce) : base(ce)
        {
            BaseVoltage = ce.BaseVoltage;
            Terminals.AddRange(ce.Terminals);
        }

        #region Properties
        public List<long> Terminals
        {
            get { return terminals; }
            set { terminals = value; }
        }

        public long BaseVoltage
        {
            get { return baseVoltage; }
            set { baseVoltage = value; }
        }
        #endregion Properties

        public override bool Equals(object obj)
		{
            if (base.Equals(obj))
            {
                ConductingEquipment x = (ConductingEquipment)obj;
                return (CompareHelper.CompareLists(x.terminals, this.terminals, true) &&
                        x.baseVoltage == this.baseVoltage);
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
                case ModelCode.CONDUCTINGEQUIPMENT_TERMINALS:
                case ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE:
					return true;

				default:
					return base.HasProperty(property);
			}
		}

		public override void GetProperty(Property prop)
		{
			switch (prop.Id)
			{
				case ModelCode.CONDUCTINGEQUIPMENT_TERMINALS:
					prop.SetValue(terminals);
					break;
                case ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE:
                    prop.SetValue(baseVoltage);
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
                case ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE:
                    baseVoltage = property.AsReference();
                    break;
                default:
                    base.SetProperty(property);
                    break;
            }
            
		}

        #endregion IAccess implementation

        #region IReference implementation

        public override bool IsReferenced
        {
            get
            {
                return terminals.Count != 0 || base.IsReferenced;
            }
        }

        public override void GetReferences(Dictionary<ModelCode, List<long>> references, TypeOfReference refType)
		{
			if (terminals == null && terminals.Count > 0 && (refType == TypeOfReference.Target || refType == TypeOfReference.Both))
			{
                references[ModelCode.CONDUCTINGEQUIPMENT_TERMINALS] = terminals.GetRange(0, terminals.Count);
			}

            if (baseVoltage != 0 && (refType == TypeOfReference.Reference || refType == TypeOfReference.Both))
            {
                references[ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE] = new List<long>();
                references[ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE].Add(baseVoltage);
            }

			base.GetReferences(references, refType);
		}

        public override void AddReference(ModelCode referenceId, long globalId)
        {
            switch (referenceId)
            {
                case ModelCode.TERMINAL_CONDUCTINGEQUIPMENT:
                    terminals.Add(globalId);
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
                case ModelCode.TERMINAL_CONDUCTINGEQUIPMENT:
                    if (terminals.Contains(globalId))
                    {
                        terminals.Remove(globalId);
                    }
                    else
                    {
                        CommonTrace.WriteTrace(CommonTrace.TraceWarning, "Entity (GID = 0x{0:x16}) doesn't contain reference 0x{1:x16}.", this.GlobalId, globalId);
                    }
                    break;

                default:
                    base.AddReference(referenceId, globalId);
                    break;
            }
        }

        #endregion IReference implementation

        #region IClonable
        public override IdentifiedObject Clone()
        {
            return new ConductingEquipment(this);
        }
        #endregion
    }
}

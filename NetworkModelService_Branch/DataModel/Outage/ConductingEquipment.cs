using FTN.Common;
using FTN.Common.GDA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace FTN.Services.NetworkModelService.DataModel.Core
{
	public class ConductingEquipment : Equipment
	{
        private List<long> terminals = new List<long>();

        public ConductingEquipment(long globalId) : base(globalId) 
		{
		}

        public List<long> Terminals
        {
            get { return terminals; }
            set { terminals = value; }
        }

        public override bool Equals(object obj)
		{
            if (base.Equals(obj))
            {
                return CompareHelper.CompareLists(((ConductingEquipment)obj).terminals, this.terminals, true);
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
                case ModelCode.CONDEQ_TERMINALS:
					return true;

				default:
					return base.HasProperty(property);
			}
		}

		public override void GetProperty(Property prop)
		{
			switch (prop.Id)
			{
				case ModelCode.CONDEQ_TERMINALS:
					prop.SetValue(terminals);
					break;

				default:
					base.GetProperty(prop);
					break;
			}
		}

		public override void SetProperty(Property property)
		{
            base.SetProperty(property);
            
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
                references[ModelCode.CONDEQ_TERMINALS] = terminals.GetRange(0, terminals.Count);
			}

			base.GetReferences(references, refType);
		}

        public override void AddReference(ModelCode referenceId, long globalId)
        {
            switch (referenceId)
            {
                case ModelCode.TERMINAL_CONDUCTINGEQ:
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
                case ModelCode.TERMINAL_CONDUCTINGEQ:
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
    }
}

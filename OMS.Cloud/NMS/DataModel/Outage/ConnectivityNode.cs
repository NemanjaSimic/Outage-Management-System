using System.Collections.Generic;
using OMS.Common.NmsContracts.GDA;
using OMS.Common.Cloud;

namespace NMS.DataModel
{
    public class ConnectivityNode : IdentifiedObject
    {
        #region Fields
        private List<long> terminals = new List<long>();
        #endregion

        public ConnectivityNode(long globalId) : base(globalId)
        {
        }

        protected ConnectivityNode(ConnectivityNode cn) : base(cn)
        {
            Terminals.AddRange(cn.Terminals);
        }

        #region Properties
        public List<long> Terminals
        {
            get { return terminals; }
            set { terminals = value; }
        }
        #endregion Properties

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return CompareHelper.CompareLists(((ConnectivityNode)obj).terminals, this.terminals, true);
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
                case ModelCode.CONNECTIVITYNODE_TERMINALS:
                    return true;

                default:
                    return base.HasProperty(property);
            }
        }

        public override void GetProperty(Property prop)
        {
            switch (prop.Id)
            {
                case ModelCode.CONNECTIVITYNODE_TERMINALS:
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
                references[ModelCode.CONNECTIVITYNODE_TERMINALS] = terminals.GetRange(0, terminals.Count);
            }

            base.GetReferences(references, refType);
        }

        public override void AddReference(ModelCode referenceId, long globalId)
        {
            switch (referenceId)
            {
                case ModelCode.TERMINAL_CONNECTIVITYNODE:
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
                case ModelCode.TERMINAL_CONNECTIVITYNODE:
                    if (terminals.Contains(globalId))
                    {
                        terminals.Remove(globalId);
                    }
                    else
                    {
                        string message = $"Entity (GID: 0x{this.GlobalId:X16}) doesn't contain reference 0x{globalId:X16}.";
                        Logger.LogWarning(message);
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
            return new ConnectivityNode(this);
        }
        #endregion
    }
}

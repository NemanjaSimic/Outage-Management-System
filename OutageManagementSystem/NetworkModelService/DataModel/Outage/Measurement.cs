using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Outage.Common;
using Outage.Common.GDA;

namespace Outage.DataModel
{
    public class Measurement : IdentifiedObject
    {
        #region Fields
        private string address;
        private bool isInput;
        private long terminal;
        #endregion

        public Measurement(long globalId) : base(globalId)
        {
        }

        protected Measurement(Measurement measurement) : base(measurement)
        {
            Address = measurement.Address;
            IsInput = measurement.IsInput;
            Terminal = measurement.Terminal;
        }

        #region Properties
        public string Address
        {
            get { return address; }
            set { address = value; }
        }
        public bool IsInput
        {
            get { return isInput; }
            set { isInput = value; }
        }
        public long Terminal
        {
            get { return terminal; }
            set { terminal = value; }
        }
        #endregion

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                Measurement x = (Measurement)obj;
                return x.address == this.address &&
                       x.isInput == this.isInput &&
                       x.terminal == this.terminal;
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
                case ModelCode.MEASUREMENT_ADDRESS:
                case ModelCode.MEASUREMENT_ISINPUT:
                case ModelCode.MEASUREMENT_TERMINAL:
                    return true;

                default:
                    return base.HasProperty(property);
            }
        }
        public override void GetProperty(Property property)
        {
            switch (property.Id)
            {
                case ModelCode.MEASUREMENT_ADDRESS:
                    property.SetValue(address);
                    break;
                case ModelCode.MEASUREMENT_ISINPUT:
                    property.SetValue(isInput);
                    break;
                case ModelCode.MEASUREMENT_TERMINAL:
                    property.SetValue(terminal);
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
                case ModelCode.MEASUREMENT_ADDRESS:
                    address = property.AsString();
                    break;
                case ModelCode.MEASUREMENT_ISINPUT:
                    isInput = property.AsBool();
                    break;
                case ModelCode.MEASUREMENT_TERMINAL:
                    terminal = property.AsReference();
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
            if (terminal != 0 && (refType == TypeOfReference.Reference || refType == TypeOfReference.Both))
            {
                references[ModelCode.MEASUREMENT_TERMINAL] = new List<long>();
                references[ModelCode.MEASUREMENT_TERMINAL].Add(terminal);
            }

            base.GetReferences(references, refType);
        }
        #endregion

        #region IClonable
        public override IdentifiedObject Clone()
        {
            return new Measurement(this);
        }
        #endregion
    }
}

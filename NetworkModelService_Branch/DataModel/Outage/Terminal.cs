using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using FTN.Common;
using FTN.Common.GDA;
using FTN.Services.NetworkModelService.DataModel.Core;

namespace FTN.Services.NetworkModelService.DataModel.Core
{
    public class Terminal : IdentifiedObject
    {
        private bool connected;

        private PhaseCode phases;

        private int sequenceNumber;

        private long conductingEquipment;

        private long connectivitiNode;

        

        public Terminal(long globalId) : base(globalId)
        {
        }


        public bool Connected
        {
            get { return connected; }
            set { connected = value; }
        }
        public PhaseCode Phases
        {
            get { return phases; }
            set { phases = value; }
        }
        public int SequenceNumber
        {
            get { return sequenceNumber; }
            set { sequenceNumber = value; }
        }

        public long ConductingEquipment
        {
            get { return conductingEquipment; }
            set { conductingEquipment = value; }
        }

        public long ConnectivityNode
        {
            get { return connectivitiNode; }
            set { connectivitiNode = value; }
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                Terminal x = (Terminal)obj;
                return (x.connected == this.connected &&
                        x.phases == this.phases &&
                        x.sequenceNumber == this.sequenceNumber &&
                        x.conductingEquipment == this.conductingEquipment &&
                        x.connectivitiNode == this.connectivitiNode
                        );
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

        public override bool HasProperty(ModelCode t)
        {
            switch (t)
            {
                case ModelCode.TERMINAL_CONDUCTINGEQ:
                case ModelCode.TERMINAL_CONNECTED:
                case ModelCode.TERMINAL_CONNECTIVITYNODE:
                case ModelCode.TERMINAL_PHASES:
                case ModelCode.TERMINAL_SEQNUMBER:
                    return true;
                default:
                    return base.HasProperty(t);
            }
        }

        public override void GetProperty(Property property)
        {
            switch (property.Id)
            {
                case ModelCode.TERMINAL_CONDUCTINGEQ:
                    property.SetValue(conductingEquipment);
                    break;

                case ModelCode.TERMINAL_CONNECTED:
                    property.SetValue(connected);
                    break;

                case ModelCode.TERMINAL_CONNECTIVITYNODE:
                    property.SetValue(connectivitiNode);
                    break;

                case ModelCode.TERMINAL_PHASES:
                    property.SetValue((short)phases);
                    break;

                case ModelCode.TERMINAL_SEQNUMBER:
                    property.SetValue(sequenceNumber);
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
                case ModelCode.TERMINAL_CONDUCTINGEQ:
                    conductingEquipment = property.AsReference();
                    break;

                case ModelCode.TERMINAL_CONNECTED:
                    connected = property.AsBool();
                    break;

                case ModelCode.TERMINAL_CONNECTIVITYNODE:
                    connectivitiNode = property.AsReference();
                    break;

                case ModelCode.TERMINAL_PHASES:
                    phases = (PhaseCode)property.AsEnum();
                    break;

                case ModelCode.TERMINAL_SEQNUMBER:
                    sequenceNumber = property.AsInt();
                    break;

                default:
                    base.SetProperty(property);
                    break;
            }
        }

        #endregion IAccess implementation

        #region IReference implementation	

        public override void GetReferences(Dictionary<ModelCode, List<long>> references, TypeOfReference refType)
        {
            if (connectivitiNode != 0 && (refType == TypeOfReference.Reference || refType == TypeOfReference.Both))
            {
                references[ModelCode.TERMINAL_CONNECTIVITYNODE] = new List<long>();
                references[ModelCode.TERMINAL_CONNECTIVITYNODE].Add(connectivitiNode);
            }

            if (conductingEquipment != 0 && (refType == TypeOfReference.Reference || refType == TypeOfReference.Both))
            {
                references[ModelCode.TERMINAL_CONDUCTINGEQ] = new List<long>();
                references[ModelCode.TERMINAL_CONDUCTINGEQ].Add(conductingEquipment);
            }

            base.GetReferences(references, refType);
        }

        #endregion IReference implementation

    }
}

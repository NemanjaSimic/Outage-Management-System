using Outage.Common;
using System;
using OMS.Common.SCADA;
using System.Runtime.Serialization;
using Common.SCADA;

namespace OMS.Common.ScadaContracts.DataContracts
{
    [DataContract]
    public class DiscretePointItem : ScadaModelPointItem, IDiscretePointItem
    {
        private ushort currentValue;

        public DiscretePointItem(ISetAlarmStrategy alarmStrategy)
            : base(alarmStrategy)
        {
        }

        [DataMember]
        public ushort MinValue { get; set; }
        [DataMember]
        public ushort MaxValue { get; set; }
        [DataMember]
        public ushort NormalValue { get; set; }
        public ushort CurrentValue
        {
            get { return currentValue; }
            set
            {
                currentValue = value;
                SetAlarms();
            }
        }

        [DataMember]
        public ushort AbnormalValue { get; set; }
        [DataMember]
        public DiscreteMeasurementType DiscreteType { get; set; }

        public override bool SetAlarms()
        {
            return alarmStrategy.SetAlarm(this);
        }

        #region IClonable

        public override IScadaModelPointItem Clone()
        {
            return this.MemberwiseClone() as IScadaModelPointItem;
        }

        #endregion IClonable

    }
}

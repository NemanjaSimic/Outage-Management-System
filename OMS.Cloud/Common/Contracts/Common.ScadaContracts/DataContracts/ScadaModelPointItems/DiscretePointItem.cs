using System;
using OMS.Common.SCADA;
using System.Runtime.Serialization;
using OMS.Common.Cloud;

namespace OMS.Common.ScadaContracts.DataContracts.ScadaModelPointItems
{
    [DataContract]
    public class DiscretePointItem : ScadaModelPointItem//, IDiscretePointItem
    {
        public DiscretePointItem(IAlarmConfigData alarmConfigData)
            : base(alarmConfigData)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";
        }

        [DataMember]
        public ushort MinValue { get; set; }
        [DataMember]
        public ushort MaxValue { get; set; }
        [DataMember]
        public ushort NormalValue { get; set; }
        [DataMember]
        public ushort CurrentValue { get; set; }
        [DataMember]
        public ushort AbnormalValue { get; set; }
        [DataMember]
        public DiscreteMeasurementType DiscreteType { get; set; }

        protected override AlarmType CheckAlarmValue()
        {
            if (!Initialized)
            {
                return AlarmType.NO_ALARM;
            }

            //ALARMS FOR DIGITAL VALUES
            if (RegisterType != PointType.DIGITAL_INPUT && RegisterType != PointType.DIGITAL_OUTPUT)
            {
                string errorMessage = $"{baseLogString} SetAlarms => PointItem [Gid: 0x{Gid:X16}, Address: {Address}] RegisterType value is invalid. Value: {RegisterType}";
                Logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            AlarmType currentAlarm;

            //VALUE IS INVALID
            if (CurrentValue < MinValue || CurrentValue > MaxValue)
            {
                currentAlarm = AlarmType.REASONABILITY_FAILURE;
            }
            //VALUE IS NOT A NORMAL VALUE -> ABNORMAL ALARM
            else if (CurrentValue != NormalValue && DiscreteType == DiscreteMeasurementType.SWITCH_STATUS)
            {
                currentAlarm = AlarmType.ABNORMAL_VALUE;
            }
            //VALUE IS NORMAL VALUE - NO ALARM
            else
            {
                currentAlarm = AlarmType.NO_ALARM;
            }

            return currentAlarm;
        }

        #region IClonable

        public override ScadaModelPointItem Clone()
        {
            return this.MemberwiseClone() as ScadaModelPointItem;
        }

        #endregion IClonable

    }
}

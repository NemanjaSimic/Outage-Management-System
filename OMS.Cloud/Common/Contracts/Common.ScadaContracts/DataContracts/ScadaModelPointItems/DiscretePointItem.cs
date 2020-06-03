using Outage.Common;
using System;
using OMS.Common.SCADA;
using System.Runtime.Serialization;

namespace OMS.Common.ScadaContracts.DataContracts.ScadaModelPointItems
{
    [DataContract]
    public class DiscretePointItem : ScadaModelPointItem, IDiscretePointItem
    {
        private ushort currentValue;

        public DiscretePointItem(IAlarmConfigData alarmConfigData)
            : base(alarmConfigData)
        {
        }

        [DataMember]
        public ushort MinValue { get; set; }
        [DataMember]
        public ushort MaxValue { get; set; }
        [DataMember]
        public ushort NormalValue { get; set; }
        [DataMember]
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
            if (!Initialized)
            {
                return false;
            }

            bool alarmChanged = false;
            AlarmType currentAlarm = Alarm;

            //ALARMS FOR DIGITAL VALUES
            if (RegisterType == PointType.DIGITAL_INPUT || RegisterType == PointType.DIGITAL_OUTPUT)
            {
                //VALUE IS INVALID
                if (CurrentValue < MinValue || CurrentValue > MaxValue)
                {
                    Alarm = AlarmType.REASONABILITY_FAILURE;
                    if (currentAlarm != Alarm)
                    {
                        alarmChanged = true;
                    }

                    //TODO: maybe throw new Exception("Invalid value.");
                }
                //VALUE IS NOT A NORMAL VALUE -> ABNORMAL ALARM
                else if (CurrentValue != NormalValue && DiscreteType == DiscreteMeasurementType.SWITCH_STATUS)
                {
                    Alarm = AlarmType.ABNORMAL_VALUE;
                    if (currentAlarm != Alarm)
                    {
                        alarmChanged = true;
                    }
                }
                //VALUE IS NORMAL VALUE - NO ALARM
                else
                {
                    Alarm = AlarmType.NO_ALARM;
                    if (currentAlarm != Alarm)
                    {
                        alarmChanged = true;
                    }
                }
            }
            else
            {
                throw new Exception($"PointItem [Gid: 0x{Gid:X16}, Address: {Address}] RegisterType value is invalid. Value: {RegisterType}");
            }

            return alarmChanged;
        }

        #region IClonable

        public override IScadaModelPointItem Clone()
        {
            return this.MemberwiseClone() as IScadaModelPointItem;
        }

        #endregion IClonable

    }
}

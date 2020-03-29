using Outage.Common;
using System;
using System.Collections.Generic;
using OMS.Common.SCADA;
using OMS.Common.NmsContracts.GDA;

namespace OMS.Cloud.SCADA.Data.Repository
{
    public class DiscreteSCADAModelPointItem : SCADAModelPointItem, IDiscreteSCADAModelPointItem
    {
        private ushort currentValue;

        public DiscreteSCADAModelPointItem(List<Property> props, ModelCode type, EnumDescs enumDescs)
            : base(props, type)
        {
            foreach (var item in props)
            {
                switch (item.Id)
                {
                    case ModelCode.DISCRETE_CURRENTOPEN:
                        CurrentValue = (ushort)((item.AsBool() == true) ? 1 : 0);
                        break;

                    case ModelCode.DISCRETE_MAXVALUE:
                        MaxValue = (ushort)item.AsInt();
                        break;

                    case ModelCode.DISCRETE_MINVALUE:
                        MinValue = (ushort)item.AsInt();
                        break;

                    case ModelCode.DISCRETE_NORMALVALUE:
                        NormalValue = (ushort)item.AsInt();
                        break;
                    case ModelCode.DISCRETE_MEASUREMENTTYPE:
                        DiscreteType = (DiscreteMeasurementType)(enumDescs.GetEnumValueFromString(ModelCode.ANALOG_SIGNALTYPE, item.AsEnum().ToString()));
                        break;

                    default:
                        break;
                }
            }

            Initialized = true;
            SetAlarms();
        }

        public ushort MinValue { get; set; }
        public ushort MaxValue { get; set; }
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
        public ushort AbnormalValue { get; set; }
        public DiscreteMeasurementType DiscreteType { get; set; }

        protected override bool SetAlarms()
        {
            if(!Initialized)
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

        public override ISCADAModelPointItem Clone()
        {
            return this.MemberwiseClone() as ISCADAModelPointItem;
        }

        #endregion IClonable

    }
}

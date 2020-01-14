using Outage.Common;
using Outage.Common.GDA;
using Outage.SCADA.SCADACommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.SCADA.SCADAData.Repository
{
    public class DiscreteSCADAModelPointItem : SCADAModelPointItem, IDiscreteSCADAModelPointItem
    {
        public DiscreteSCADAModelPointItem()
            : base()
        {
        }

        public DiscreteSCADAModelPointItem(List<Property> props, ModelCode type)
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

                    default:
                        break;
                }
            }
        }

        public ushort MinValue { get; set; }
        public ushort MaxValue { get; set; }
        public ushort NormalValue { get; set; }
        public ushort CurrentValue { get; set; }

        public ushort AbnormalValue { get; set; }

        public override bool SetAlarms()
        {
            bool alarmChanged = false;
            AlarmType currentAlarm = Alarm;

            //ALARMS FOR DIGITAL VALUES
            if (RegisterType == PointType.DIGITAL_INPUT || RegisterType == PointType.DIGITAL_OUTPUT)
            {
                //VALUE IS INVALID
                if (CurrentValue < MinValue || CurrentValue > MaxValue)
                {
                    Alarm = AlarmType.ABNORMAL_VALUE;
                    if (currentAlarm != Alarm)
                    {
                        alarmChanged = true;
                    }

                    //TODO: maybe throw new Exception("Invalid value.");
                }
                //VALUE IS NOT A NORMAL VALUE -> ABNORMAL ALARM
                else if (CurrentValue != NormalValue)
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

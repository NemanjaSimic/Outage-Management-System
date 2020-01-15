using Outage.Common;
using Outage.Common.GDA;
using Outage.SCADA.SCADACommon;
using Outage.SCADA.SCADAData.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.SCADA.SCADAData.Repository
{
    public class AnalogSCADAModelPointItem : SCADAModelPointItem, IAnalogSCADAModelPointItem
    {
        public AnalogSCADAModelPointItem() 
            : base()
        {
            ScaleFactor = 1;    //TODO: za sad cisto da bi bilo popunjeno (zahteva promene na NMS)
            Deviation = 0;      //TODO: za sad cisto da bi bilo popunjeno (zahteva promene na NMS)
        }

        public AnalogSCADAModelPointItem(List<Property> props, ModelCode type)
            : base(props, type)
        {
            ScaleFactor = 1;    //TODO: za sad cisto da bi bilo popunjeno (zahteva promene na NMS)
            Deviation = 0;      //TODO: za sad cisto da bi bilo popunjeno (zahteva promene na NMS)

            foreach (var item in props)
            {
                switch (item.Id)
                {
                    case ModelCode.ANALOG_CURRENTVALUE:
                        CurrentEguValue = item.AsFloat();
                        break;

                    case ModelCode.ANALOG_MAXVALUE:
                        EGU_Max = item.AsFloat();
                        break;

                    case ModelCode.ANALOG_MINVALUE:
                        EGU_Min = item.AsFloat();
                        break;

                    case ModelCode.ANALOG_NORMALVALUE:
                        NormalValue = item.AsFloat();
                        break;

                    //case ModelCode.ANALOG_SCALEFACTOR:
                    //    ScaleFactor = item.AsFloat();
                    //    break;

                    //case ModelCode.ANALOG_DEVIATION:
                    //    Deviation = item.AsFloat();
                    //    break;

                    default:
                        break;
                }

            }
                         
        }

        public double NormalValue { get; set; }
        public double CurrentEguValue { get; set; }
        public double EGU_Min { get; set; }
        public double EGU_Max { get; set; }
        public float ScaleFactor { get; set; }
        public float Deviation { get; set; }
        public AnalogType AnalogType { get; set; }

        public int CurrentRawValue
        {
            get
            {
                return EguToRawValueConversion(CurrentEguValue);
            }
        }

        public int MinRawValue
        {
            get
            {
                return EguToRawValueConversion(CurrentEguValue);
            }
        }

        public int MaxRawValue
        {
            get
            {
                return EguToRawValueConversion(CurrentEguValue);
            }
        }


        public override bool SetAlarms()
        {
            bool alarmChanged = false;
            AlarmType currentAlarm = Alarm;
            ushort LowLimit = 0;
            ushort HighLimit = 0;
            if (AnalogType == AnalogType.Power)
            {
                LowLimit = AlarmConfigData.Instance.LowPowerLimit;
                HighLimit = AlarmConfigData.Instance.HighPowerLimit;
            }
            else if(AnalogType == AnalogType.Voltage)
            {
                LowLimit = AlarmConfigData.Instance.LowVoltageLimit;
                HighLimit = AlarmConfigData.Instance.HighVolageLimit;
            }
            //ALARMS FOR ANALOG VALUES
            if (RegisterType == PointType.ANALOG_INPUT || RegisterType == PointType.ANALOG_OUTPUT)
            {
                //VALUE IS INVALID
                if (CurrentRawValue < MinRawValue || CurrentRawValue > MaxRawValue)
                {
                    Alarm = AlarmType.ABNORMAL_VALUE;
                    if (currentAlarm != Alarm)
                    {
                        alarmChanged = true;
                    }

                    //TODO: maybe throw new Exception("Invalid value");
                }
                else if(CurrentEguValue < EGU_Min || CurrentEguValue > EGU_Max)
                {
                    Alarm = AlarmType.REASONABILITY_FAILURE;
                    if (currentAlarm != Alarm)
                    {
                        alarmChanged = true;
                    }
                }
                else if (CurrentEguValue > EGU_Min && CurrentEguValue < LowLimit)
                {
                    Alarm = AlarmType.LOW_ALARM;
                    if (currentAlarm != Alarm)
                    {
                        alarmChanged = true;
                    }
                }
                else if (CurrentEguValue < EGU_Max && CurrentEguValue > HighLimit)
                {
                    Alarm = AlarmType.HIGH_ALARM;
                    if (currentAlarm != Alarm)
                    {
                        alarmChanged = true;
                    }
                }
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


        public double RawToEguValueConversion(int rawValue)
        {
            //TODO: implement with Deviation and ScaleFactor
            return rawValue;
        }

        public int EguToRawValueConversion(double eguValue)
        {
            //TODO: implement with Deviation and ScaleFactor
            return (int)eguValue;
        }

        #region IClonable

        public override ISCADAModelPointItem Clone()
        {
            return this.MemberwiseClone() as ISCADAModelPointItem;
        }

        #endregion IClonable
    }
}

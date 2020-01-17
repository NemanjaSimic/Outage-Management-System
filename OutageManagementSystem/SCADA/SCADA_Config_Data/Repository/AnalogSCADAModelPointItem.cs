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
    public class AnalogSCADAModelPointItem : SCADAModelPointItem, IAnalogSCADAModelPointItem
    {
        public AnalogSCADAModelPointItem() 
            : base()
        {
            ScaleFactor = 1; //TODO: pre svih ostalih
            Deviation = 0;  //TODO: pre svih ostalih
        }

        public AnalogSCADAModelPointItem(List<Property> props, ModelCode type)
            : base(props, type)
        {
            ScaleFactor = 1; //TODO: pre svih ostalih
            Deviation = 0;  //TODO: pre svih ostalih

            foreach (var item in props)
            {
                switch (item.Id)
                {
                    case ModelCode.ANALOG_CURRENTVALUE:
                        CurrentRawValue = EguToRawValueConversion(item.AsFloat());
                        break;

                    case ModelCode.ANALOG_MAXVALUE:
                        MaxValue = EguToRawValueConversion(item.AsFloat());
                        EGU_Max = item.AsFloat();
                        break;

                    case ModelCode.ANALOG_MINVALUE:
                        MinValue = EguToRawValueConversion(item.AsFloat());
                        EGU_Min = item.AsFloat();
                        break;

                    case ModelCode.ANALOG_NORMALVALUE:
                        NormalValue = item.AsFloat();
                        break;

                    case ModelCode.ANALOG_SCALINGFACTOR:
                        ScaleFactor = item.AsFloat();
                        break;

                    case ModelCode.ANALOG_DEVIATION:
                        Deviation = item.AsFloat();
                        break;

                    default:
                        break;
                }

            }
            
            LowLimit = EGU_Min + 5; //todo: config
            HighLimit = EGU_Max - 5; //todo: config                
        }

        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public int CurrentRawValue { get; set; }
        public double NormalValue { get; set; }
        public double CurrentEguValue
        {
            get
            {
                return RawToEguValueConversion(CurrentRawValue);
            }
        }
        public float ScaleFactor { get; set; }
        public float Deviation { get; set; }

        public double EGU_Min { get; set; }
        public double EGU_Max { get; set; }
        public double HighLimit { get; set; }
        public double LowLimit { get; set; }

        public override bool SetAlarms()
        {
            bool alarmChanged = false;
            AlarmType currentAlarm = Alarm;

            //ALARMS FOR ANALOG VALUES
            if (RegisterType == PointType.ANALOG_INPUT || RegisterType == PointType.ANALOG_OUTPUT)
            {
                //VALUE IS INVALID
                if (CurrentRawValue < MinValue || CurrentRawValue > MaxValue)
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
            //TODO: implement
            return rawValue;
        }

        public int EguToRawValueConversion(double eguValue)
        {
            //TODO: implement
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

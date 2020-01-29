using Outage.Common;
using Outage.Common.GDA;
using Outage.SCADA.SCADACommon;
using Outage.SCADA.SCADAData.Configuration;
using System;
using System.Collections.Generic;

namespace Outage.SCADA.SCADAData.Repository
{
    public class AnalogSCADAModelPointItem : SCADAModelPointItem, IAnalogSCADAModelPointItem
    {
        public AnalogSCADAModelPointItem() 
            : base()
        {
        }

        public AnalogSCADAModelPointItem(List<Property> props, ModelCode type, EnumDescs enumDescs)
            : base(props, type)
        {
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

                    case ModelCode.ANALOG_SCALINGFACTOR:
                        ScalingFactor = item.AsFloat();
                        break;

                    case ModelCode.ANALOG_DEVIATION:
                        Deviation = item.AsFloat();
                        break;

                    case ModelCode.ANALOG_SIGNALTYPE:
                        AnalogType = (AnalogMeasurementType)(enumDescs.GetEnumValueFromString(ModelCode.ANALOG_SIGNALTYPE, item.AsEnum().ToString()));
                        break;

                    default:
                        break;
                }
            }                       
        }

        public float NormalValue { get; set; }
        public float CurrentEguValue { get; set; }
        public float EGU_Min { get; set; }
        public float EGU_Max { get; set; }
        public float ScalingFactor { get; set; }
        public float Deviation { get; set; }
        public AnalogMeasurementType AnalogType { get; set; }

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
            ushort LowLimit;
            ushort HighLimit;
            AlarmType currentAlarm = Alarm;

            if (AnalogType == AnalogMeasurementType.POWER)
            {
                LowLimit = AlarmConfigData.Instance.LowPowerLimit;
                HighLimit = AlarmConfigData.Instance.HighPowerLimit;
            }
            else if(AnalogType == AnalogMeasurementType.VOLTAGE)
            {
                LowLimit = AlarmConfigData.Instance.LowVoltageLimit;
                HighLimit = AlarmConfigData.Instance.HighVolageLimit;
            }
            else
            {
                throw new Exception($"Analog measurement is of type: {AnalogType} which is not supported for alarming.");
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

        #region Conversions
        
        public float RawToEguValueConversion(int rawValue)
        {
            float eguValue = ((ScalingFactor * rawValue) + Deviation);

            if(eguValue > float.MaxValue || eguValue < float.MinValue)
            {
                throw new Exception($"Egu value: {eguValue} is out of float data type boundaries [{float.MinValue}, {float.MaxValue}]");
            }

            return eguValue;
        }

        public int EguToRawValueConversion(float eguValue)
        {
            if(ScalingFactor == 0)
            {
                throw new DivideByZeroException($"Scaling factor is zero.");
            }

            int rawValue = (int)((eguValue - Deviation) / ScalingFactor);

            return rawValue;
        }

        #endregion

        #region IClonable

        public override ISCADAModelPointItem Clone()
        {
            return this.MemberwiseClone() as ISCADAModelPointItem;
        }

        #endregion IClonable
    }
}

using Outage.SCADA.SCADA_Common;
using System;

namespace Outage.SCADA.SCADA_Config_Data.Configuration
{
    public class ConfigItem : IConfigItem,ICloneable
    {
        public PointType RegistarType { get; set; }
        public ushort Address { get; set; }
        public float MinValue { get; set; }
        public float MaxValue { get; set; }
        public float DefaultValue { get; set; }
        public double ScaleFactor { get; set; }
        public double Deviation { get; set; }
        public double EGU_Min { get; set; }
        public double EGU_Max { get; set; }

        public ushort AbnormalValue { get; set; }

        public double HighLimit { get; set; }
        public double LowLimit { get; set; }
        public long Gid { get; set; }
        public string Name { get; set; }
        public float CurrentValue { get; set; }
        public AlarmType Alarm { get; set; }
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        private PointType GetRegistryType(string registryTypeName)
        {
            PointType registryType;
            switch (registryTypeName)
            {
                case "DO_REG":
                    registryType = PointType.DIGITAL_OUTPUT;
                    break;

                case "DI_REG":
                    registryType = PointType.DIGITAL_INPUT;
                    break;

                case "IN_REG":
                    registryType = PointType.ANALOG_INPUT;
                    break;

                case "HR_INT":
                    registryType = PointType.ANALOG_OUTPUT;
                    break;

                default:
                    registryType = PointType.HR_LONG;
                    break;
            }
            return registryType;
        }

        public bool SetAlarms()
        {

            bool AlarmChanged = false;
            AlarmType CurrentAlarm = Alarm;

            //ALARMS FOR ANALOG VALUES
            if (RegistarType == PointType.ANALOG_INPUT || RegistarType == PointType.ANALOG_OUTPUT)
            {
                //VALUE IS ABOVE EGU_MAX, BUT BELOW HIGHEST POSSIBLE VALUE - ABNORMAL
                if (CurrentValue > EGU_Max && CurrentValue < HighLimit)
                {                  
                    Alarm = AlarmType.ABNORMAL_VALUE;
                    if (CurrentAlarm != Alarm)
                    {
                        AlarmChanged = true;
                    }

                    return AlarmChanged;
                }
                //VALUE IS ABOVE HIGHEST POSSIBLE VALUE - HIGH ALARM
                else if (CurrentValue > HighLimit)
                {
                    Alarm = AlarmType.HIGH_ALARM;
                    if (CurrentAlarm != Alarm)
                    {
                        AlarmChanged = true;
                    }

                    return AlarmChanged;
                }
                //VALUE IS BELOW EGU_MIN, BUT ABOVE LOWEST POSSIBLE VALUE - ABNORMAL
                else if (CurrentValue < EGU_Min && CurrentValue > LowLimit)
                {
                    Alarm = AlarmType.ABNORMAL_VALUE;
                    if (CurrentAlarm != Alarm)
                    {
                        AlarmChanged = true;
                    }

                    return AlarmChanged;
                }
                //VALUE IS BELOW LOWEST POSSIBLE VALUE - LOW ALARM
                else if (CurrentValue < LowLimit)
                {
                    Alarm = AlarmType.LOW_ALARM;
                    if (CurrentAlarm != Alarm)
                    {
                        AlarmChanged = true;
                    }

                    return AlarmChanged;
                }
                //VALUE IS REASONABLE - NO ALARM
                else
                {
                    Alarm = AlarmType.NO_ALARM;
                    if (CurrentAlarm != Alarm)
                    {
                        AlarmChanged = true;
                    }

                    return AlarmChanged;
                }
            }
            //ALARMS FOR DIGITAL VALUES
            else if (RegistarType == PointType.DIGITAL_INPUT || RegistarType == PointType.DIGITAL_OUTPUT)
            {
                //VALUE IS NOT A DEFAULT VALUE - ABNORMAL
                if (CurrentValue != DefaultValue)
                {
                    Alarm = AlarmType.ABNORMAL_VALUE;
                    if (CurrentAlarm != Alarm)
                    {
                        AlarmChanged = true;
                    }

                    return AlarmChanged;
                }
                //VALUE IS DEFAULT VALUE - NO ALARM
                else
                {
                    Alarm = AlarmType.NO_ALARM;
                    if (CurrentAlarm != Alarm)
                    {
                        AlarmChanged = true;
                    }

                    return AlarmChanged;
                }
            }
            else
            {
                throw new Exception("PointType value is invalid");
            }
        }
    }
}
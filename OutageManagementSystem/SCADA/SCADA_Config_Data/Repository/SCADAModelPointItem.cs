using Outage.Common;
using Outage.Common.GDA;
using Outage.SCADA.SCADACommon;
using System;
using System.Collections.Generic;

namespace Outage.SCADA.SCADAData.Repository
{
    public class SCADAModelPointItem : ISCADAModelPointItem //, ICloneable
    {
        private ILogger logger;

        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        public long Gid { get; set; }
        public PointType RegistarType { get; set; }
        public string Name { get; set; }
        public float CurrentValue { get; set; }
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
        public AlarmType Alarm { get; set; }

        public SCADAModelPointItem()
        {
        }

        public SCADAModelPointItem(List<Property> props, ModelCode type)
        {
            foreach (var item in props)
            {
                switch (item.Id)
                {
                    case ModelCode.IDOBJ_GID:
                        Gid = item.AsLong();
                        break;

                    case ModelCode.IDOBJ_NAME:
                        Name = item.AsString();
                        break;

                    case ModelCode.DISCRETE_CURRENTOPEN:
                        CurrentValue = (item.AsBool() == true) ? 1 : 0;
                        break;

                    case ModelCode.DISCRETE_MAXVALUE:
                        MaxValue = item.AsInt();
                        break;

                    case ModelCode.DISCRETE_MINVALUE:
                        MinValue = item.AsInt();
                        break;

                    case ModelCode.DISCRETE_NORMALVALUE:
                        DefaultValue = item.AsInt();
                        break;

                    case ModelCode.MEASUREMENT_ADDRESS:
                        if (ushort.TryParse(item.AsString(), out ushort address))
                        {
                            Address = address;
                        }
                        else
                        {
                            string message = "SCADAModelPointItem constructor => Address is either not defined or is invalid.";
                            Logger.LogError(message);
                            throw new ArgumentException(message);
                        }
                        break;

                    case ModelCode.MEASUREMENT_ISINPUT:
                        if (type == ModelCode.ANALOG)
                        {
                            RegistarType = (item.AsBool() == true) ? PointType.ANALOG_INPUT : PointType.ANALOG_OUTPUT;
                        }
                        else if (type == ModelCode.DISCRETE)
                        {
                            RegistarType = (item.AsBool() == true) ? PointType.DIGITAL_INPUT : PointType.DIGITAL_OUTPUT;
                        }
                        else
                        {
                            string message = "SCADAModelPointItem constructor => ModelCode type is neither ANALOG nor DISCRETE.";
                            Logger.LogError(message);
                            throw new ArgumentException(message);
                        }
                        break;

                    case ModelCode.ANALOG_CURRENTVALUE:
                        CurrentValue = item.AsFloat();
                        break;

                    case ModelCode.ANALOG_MAXVALUE:
                        MaxValue = item.AsFloat();
                        break;

                    case ModelCode.ANALOG_MINVALUE:
                        MinValue = item.AsFloat();
                        break;

                    case ModelCode.ANALOG_NORMALVALUE:
                        DefaultValue = item.AsFloat();
                        break;

                    default:
                        break;
                }

                if (RegistarType == PointType.ANALOG_INPUT || RegistarType == PointType.ANALOG_OUTPUT)
                {
                    LowLimit = EGU_Min + 200;
                    HighLimit = EGU_Max - 200;
                }
                else if (RegistarType == PointType.DIGITAL_INPUT || RegistarType == PointType.DIGITAL_OUTPUT)
                {
                    LowLimit = 0;
                    HighLimit = 1;
                }
            }
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

        #region IClonable

        public ISCADAModelPointItem Clone()
        {
            return this.MemberwiseClone() as ISCADAModelPointItem;
        }

        #endregion IClonable

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
    }
}
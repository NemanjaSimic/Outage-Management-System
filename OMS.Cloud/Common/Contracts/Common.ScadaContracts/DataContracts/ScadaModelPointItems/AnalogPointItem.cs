using OMS.Common.Cloud;
using OMS.Common.Cloud.Logger;
using OMS.Common.SCADA;
using System;
using System.Runtime.Serialization;

namespace OMS.Common.ScadaContracts.DataContracts.ScadaModelPointItems
{
    [DataContract]
    public class AnalogPointItem : ScadaModelPointItem, IAnalogPointItem
    {
        private float currentEguValue;

        public AnalogPointItem(IAlarmConfigData alarmConfigData)
            : base(alarmConfigData)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            ScalingFactor = 1;
            Deviation = 0;
        }

        [DataMember]
        public float NormalValue { get; set; }
        [DataMember]
        public float CurrentEguValue
        {
            get { return currentEguValue; }
            set
            {
                currentEguValue = value;
                SetAlarms();
            }
        }
        
        [DataMember]
        public float EGU_Min { get; set; }
        [DataMember]
        public float EGU_Max { get; set; }
        [DataMember]
        public float ScalingFactor { get; set; }
        [DataMember]
        public float Deviation { get; set; }
        [DataMember]
        public AnalogMeasurementType AnalogType { get; set; }
        
        [IgnoreDataMember]
        public int CurrentRawValue
        {
            get
            {
                if(!Initialized)
                {
                    return 0;
                }

                return EguToRawValueConversion(CurrentEguValue);
            }
        }

        [IgnoreDataMember]
        public int MinRawValue
        {
            get
            {
                if (!Initialized)
                {
                    return 0;
                }

                return EguToRawValueConversion(CurrentEguValue);
            }
        }

        [IgnoreDataMember]
        public int MaxRawValue
        {
            get
            {
                if (!Initialized)
                {
                    return 0;
                }

                return EguToRawValueConversion(CurrentEguValue);
            }
        }

        public override bool SetAlarms()
        {
            if (!Initialized)
            {
                return false;
            }

            bool alarmChanged = false;
            float LowLimit;
            float HighLimit;
            AlarmType currentAlarm = Alarm;

            if (AnalogType == AnalogMeasurementType.POWER)
            {
                LowLimit = alarmConfigData.LowPowerLimit;
                HighLimit = alarmConfigData.HighPowerLimit;
            }
            else if (AnalogType == AnalogMeasurementType.VOLTAGE)
            {
                LowLimit = alarmConfigData.LowVoltageLimit;
                HighLimit = alarmConfigData.HighVolageLimit;
            }
            else if (AnalogType == AnalogMeasurementType.CURRENT)
            {
                LowLimit = alarmConfigData.LowCurrentLimit;
                HighLimit = alarmConfigData.HighCurrentLimit;
            }
            else
            {
                string message = $"{baseLogString} SetAlarms => Analog measurement is of type: {AnalogType} which is not supported for alarming. Gid: 0x{Gid:X16}, Addres: {Address}, Name: {Name}, RegisterType: {RegisterType}, Initialized: {Initialized}";
                Logger.LogError(message);
                throw new Exception(message);
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
                else if (CurrentEguValue < EGU_Min || CurrentEguValue > EGU_Max)
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
                string errorMessage = $"{baseLogString} SetAlarms => PointItem [Gid: 0x{Gid:X16}, Address: {Address}] RegisterType value is invalid. Value: {RegisterType}";
                Logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            return alarmChanged;
        }

        #region Conversions

        public float RawToEguValueConversion(int rawValue)
        {
            if(!Initialized)
            {
                Logger.LogDebug($"{baseLogString} EguToRawValueConversion => Method called before PointItem was initialized. Gid: 0x{Gid:X16}, Addres: {Address}, Name: {Name}, RegisterType: {RegisterType}, Initialized: {Initialized}");
                return 0;
            }

            float eguValue = ((ScalingFactor * rawValue) + Deviation);

            if (eguValue > float.MaxValue || eguValue < float.MinValue)
            {
                throw new Exception($"{baseLogString} RawToEguValueConversion => Egu value: {eguValue} is out of float data type boundaries [{float.MinValue}, {float.MaxValue}]. Gid: 0x{Gid:X16}, Addres: {Address}, Name: {Name}, RegisterType: {RegisterType}, Initialized: {Initialized}");
            }

            return eguValue;
        }

        public int EguToRawValueConversion(float eguValue)
        {
            if (!Initialized)
            {
                Logger.LogDebug($"{baseLogString} EguToRawValueConversion => Method called before PointItem was initialized. Gid: 0x{Gid:X16}, Addres: {Address}, Name: {Name}, RegisterType: {RegisterType}, Initialized: {Initialized}");
                return 0;
            }

            //TODO: veoma cudno ponasanje - conditional breakpoint sa uslovom 'ScalingFactor == 0', po zaustavljanju ScalingFactor ima vrednost 1, odustajem razumevanja baga dok ne ispolji zacajnije posledice - donji fix resava slucaj
            if (ScalingFactor == 0)
            {
                ScalingFactor = 1;
                //throw new DivideByZeroException($"Scaling factor is zero."); 
                Logger.LogVerbose($"{baseLogString} EguToRawValueConversion => Scaling factor is zero, and set to 1 to prevent throw of DivideByZeroException. Gid: 0x{Gid:X16}, Addres: {Address}, Name: {Name}, RegisterType: {RegisterType}, Initialized: {Initialized}");
            }

            int rawValue = (int)((eguValue - Deviation) / ScalingFactor);

            return rawValue;
        }

        #endregion

        #region IClonable

        public override IScadaModelPointItem Clone()
        {
            return this.MemberwiseClone() as IScadaModelPointItem;
        }

        #endregion IClonable
    }
}

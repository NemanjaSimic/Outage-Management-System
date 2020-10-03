using OMS.Common.Cloud;
using OMS.Common.SCADA;
using System;
using System.Runtime.Serialization;

namespace OMS.Common.ScadaContracts.DataContracts.ScadaModelPointItems
{
    [DataContract]
    public class AnalogPointItem : ScadaModelPointItem//, IAnalogPointItem
    { 
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
        public float CurrentEguValue { get; set; }
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

        protected override AlarmType CheckAlarmValue()
        {
            if (!Initialized)
            {
                return AlarmType.NO_ALARM;
            }

            float LowLimit;
            float HighLimit;
            
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
            else  if(AnalogType == AnalogMeasurementType.FEEDER_CURRENT)
            {
                LowLimit = alarmConfigData.LowFeederCurrentLimit;
                HighLimit = alarmConfigData.HighFeederCurrentLimit;
            }
            else
            {
                string message = $"{baseLogString} SetAlarms => Analog measurement is of type: {AnalogType} which is not supported for alarming. Gid: 0x{Gid:X16}, Addres: {Address}, Name: {Name}, RegisterType: {RegisterType}, Initialized: {Initialized}";
                Logger.LogError(message);
                throw new Exception(message);
            }

            //ALARMS FOR ANALOG VALUES
            if (RegisterType != PointType.ANALOG_INPUT && RegisterType != PointType.ANALOG_OUTPUT)
            {
                string errorMessage = $"{baseLogString} SetAlarms => PointItem [Gid: 0x{Gid:X16}, Address: {Address}] RegisterType value is invalid. Value: {RegisterType}";
                Logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }

            AlarmType currentAlarm;

            //VALUE IS INVALID
            if (CurrentRawValue < MinRawValue || CurrentRawValue > MaxRawValue)
            {
                currentAlarm = AlarmType.ABNORMAL_VALUE;
            }
            else if (CurrentEguValue < EGU_Min || CurrentEguValue > EGU_Max)
            {
                currentAlarm = AlarmType.REASONABILITY_FAILURE;
            }
            else if (CurrentEguValue > EGU_Min && CurrentEguValue < LowLimit)
            {
                currentAlarm = AlarmType.LOW_ALARM;
            }
            else if (CurrentEguValue < EGU_Max && CurrentEguValue > HighLimit)
            {
                currentAlarm = AlarmType.HIGH_ALARM;
            }
            else
            {
                currentAlarm = AlarmType.NO_ALARM;
            }

            return currentAlarm;
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

            //veoma cudno ponasanje - conditional breakpoint sa uslovom 'ScalingFactor == 0', po zaustavljanju ScalingFactor ima vrednost 1, odustajem razumevanja baga dok ne ispolji zacajnije posledice - donji fix resava slucaj
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

        public override ScadaModelPointItem Clone()
        {
            return this.MemberwiseClone() as ScadaModelPointItem;
        }

        #endregion IClonable
    }
}

using OMS.Common.SCADA;
using OMS.Common.ScadaContracts.DataContracts;
using Outage.Common;
using SCADA.ModelProviderImplementation.Config;
using System;

namespace SCADA.ModelProviderImplementation.AlarmStrategies
{
    internal class AnalogAlarmStrategy : ISetAlarmStrategy
    {
        public bool SetAlarm(IScadaModelPointItem pointItem)
        {
            if (!pointItem.Initialized || !(pointItem is IAnalogPointItem analogPointItem))
            {
                return false;
            }

            bool alarmChanged = false;
            float LowLimit;
            float HighLimit;
            AlarmType currentAlarm = analogPointItem.Alarm;

            if (analogPointItem.AnalogType == AnalogMeasurementType.POWER)
            {
                LowLimit = AlarmConfigData.Instance.LowPowerLimit;
                HighLimit = AlarmConfigData.Instance.HighPowerLimit;
            }
            else if (analogPointItem.AnalogType == AnalogMeasurementType.VOLTAGE)
            {
                LowLimit = AlarmConfigData.Instance.LowVoltageLimit;
                HighLimit = AlarmConfigData.Instance.HighVolageLimit;
            }
            else if (analogPointItem.AnalogType == AnalogMeasurementType.CURRENT)
            {
                LowLimit = AlarmConfigData.Instance.LowCurrentLimit;
                HighLimit = AlarmConfigData.Instance.HighCurrentLimit;
            }
            else
            {
                throw new Exception($"Analog measurement is of type: {analogPointItem.AnalogType} which is not supported for alarming.");
            }

            //ALARMS FOR ANALOG VALUES
            if (analogPointItem.RegisterType == PointType.ANALOG_INPUT || analogPointItem.RegisterType == PointType.ANALOG_OUTPUT)
            {
                //VALUE IS INVALID
                if (analogPointItem.CurrentRawValue < analogPointItem.MinRawValue || analogPointItem.CurrentRawValue > analogPointItem.MaxRawValue)
                {
                    analogPointItem.Alarm = AlarmType.ABNORMAL_VALUE;
                    if (currentAlarm != analogPointItem.Alarm)
                    {
                        alarmChanged = true;
                    }

                    //TODO: maybe throw new Exception("Invalid value");
                }
                else if (analogPointItem.CurrentEguValue < analogPointItem.EGU_Min || analogPointItem.CurrentEguValue > analogPointItem.EGU_Max)
                {
                    analogPointItem.Alarm = AlarmType.REASONABILITY_FAILURE;
                    if (currentAlarm != analogPointItem.Alarm)
                    {
                        alarmChanged = true;
                    }
                }
                else if (analogPointItem.CurrentEguValue > analogPointItem.EGU_Min && analogPointItem.CurrentEguValue < LowLimit)
                {
                    analogPointItem.Alarm = AlarmType.LOW_ALARM;
                    if (currentAlarm != analogPointItem.Alarm)
                    {
                        alarmChanged = true;
                    }
                }
                else if (analogPointItem.CurrentEguValue < analogPointItem.EGU_Max && analogPointItem.CurrentEguValue > HighLimit)
                {
                    analogPointItem.Alarm = AlarmType.HIGH_ALARM;
                    if (currentAlarm != analogPointItem.Alarm)
                    {
                        alarmChanged = true;
                    }
                }
                else
                {
                    analogPointItem.Alarm = AlarmType.NO_ALARM;
                    if (currentAlarm != analogPointItem.Alarm)
                    {
                        alarmChanged = true;
                    }
                }
            }
            else
            {
                throw new Exception($"PointItem [Gid: 0x{analogPointItem.Gid:X16}, Address: {analogPointItem.Address}] RegisterType value is invalid. Value: {analogPointItem.RegisterType}");
            }

            return alarmChanged;
        }
    }
}

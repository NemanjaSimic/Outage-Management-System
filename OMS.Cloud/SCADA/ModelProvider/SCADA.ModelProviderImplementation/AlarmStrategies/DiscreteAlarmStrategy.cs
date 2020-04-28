using OMS.Common.SCADA;
using OMS.Common.ScadaContracts.DataContracts;
using Outage.Common;
using System;

namespace SCADA.ModelProviderImplementation.AlarmStrategies
{
    internal class DiscreteAlarmStrategy : ISetAlarmStrategy
    {
        public bool SetAlarm(IScadaModelPointItem pointItem)
        {
            if (!pointItem.Initialized || !(pointItem is IDiscretePointItem discretePointItem))
            {
                return false;
            }

            bool alarmChanged = false;
            AlarmType currentAlarm = discretePointItem.Alarm;

            //ALARMS FOR DIGITAL VALUES
            if (discretePointItem.RegisterType == PointType.DIGITAL_INPUT || discretePointItem.RegisterType == PointType.DIGITAL_OUTPUT)
            {
                //VALUE IS INVALID
                if (discretePointItem.CurrentValue < discretePointItem.MinValue || discretePointItem.CurrentValue > discretePointItem.MaxValue)
                {
                    discretePointItem.Alarm = AlarmType.REASONABILITY_FAILURE;
                    if (currentAlarm != discretePointItem.Alarm)
                    {
                        alarmChanged = true;
                    }

                    //TODO: maybe throw new Exception("Invalid value.");
                }
                //VALUE IS NOT A NORMAL VALUE -> ABNORMAL ALARM
                else if (discretePointItem.CurrentValue != discretePointItem.NormalValue && discretePointItem.DiscreteType == DiscreteMeasurementType.SWITCH_STATUS)
                {
                    discretePointItem.Alarm = AlarmType.ABNORMAL_VALUE;
                    if (currentAlarm != discretePointItem.Alarm)
                    {
                        alarmChanged = true;
                    }
                }
                //VALUE IS NORMAL VALUE - NO ALARM
                else
                {
                    discretePointItem.Alarm = AlarmType.NO_ALARM;
                    if (currentAlarm != discretePointItem.Alarm)
                    {
                        alarmChanged = true;
                    }
                }
            }
            else
            {
                throw new Exception($"PointItem [Gid: 0x{discretePointItem.Gid:X16}, Address: {discretePointItem.Address}] RegisterType value is invalid. Value: {discretePointItem.RegisterType}");
            }

            return alarmChanged;
        }
    }
}

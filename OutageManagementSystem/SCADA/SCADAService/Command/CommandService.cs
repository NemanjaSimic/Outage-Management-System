using System;
using Outage.Common;
using Outage.Common.ServiceContracts.SCADA;
using Outage.SCADA.ModBus;
using Outage.SCADA.ModBus.Connection;
using Outage.SCADA.ModBus.FunctionParameters;
using Outage.SCADA.ModBus.ModbusFuntions;
using Outage.SCADA.SCADACommon;
using Outage.SCADA.SCADAData.Repository;

namespace Outage.SCADA.SCADAService.Command
{
    public class CommandService : ISCADACommand
    {
        private ILogger logger;

        private FunctionExecutor functionExecutor = FunctionExecutor.Instance;
        private SCADAModel scadaModel = SCADAModel.Instance;

        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        public bool SendAnalogCommand(long gid, float commandingValue)
        {
            bool success;

            if (scadaModel.CurrentScadaModel.TryGetValue(gid, out ISCADAModelPointItem pointItem))
            {
                if (pointItem.RegistarType == PointType.ANALOG_OUTPUT)
                {
                    int modbusValue = (int)commandingValue; //TODO: EGU convertion...
                    success = SendCommand(pointItem, modbusValue);
                }
                else
                {
                    success = false;
                    string message = $"RegistarType of entity with gid: 0x{gid:X16} is not ANALOG_OUTPUT.";
                    Logger.LogError(message);
                }
            }
            else
            {
                success = false;
                string message = $"Entity with gid: 0x{gid:X16} does not exist in current SCADA model.";
                Logger.LogError(message);
            }

            return success;
        }

        public bool SendDiscreteCommand(long gid, ushort commandingValue)
        {
            bool success;

            if (scadaModel.CurrentScadaModel.TryGetValue(gid, out ISCADAModelPointItem pointItem))
            {
                if (pointItem.RegistarType == PointType.DIGITAL_OUTPUT)
                {
                    success = SendCommand(pointItem, commandingValue);
                }
                else
                {
                    success = false;
                    string message = $"RegistarType of entity with gid: 0x{gid:X16} is not DIGITAL_OUTPUT.";
                    Logger.LogError(message);
                }
            }
            else
            {
                success = false;
                string message = $"Entity with gid: 0x{gid:X16} does not exist in current SCADA model.";
                Logger.LogError(message);
            }

            return success;
        }

        private bool SendCommand(ISCADAModelPointItem pointItem, object commandingValue)
        {
            bool success = false;
            ushort length = 6;
            ModbusWriteCommandParameters modbusWriteCommandParams;

            try
            {
                if (pointItem.RegistarType == PointType.ANALOG_OUTPUT && commandingValue is int analogCommandingValue)
                {
                    modbusWriteCommandParams = new ModbusWriteCommandParameters(length,
                                                                           (byte)ModbusFunctionCode.WRITE_SINGLE_REGISTER,
                                                                           pointItem.Address,
                                                                           analogCommandingValue);

                    Logger.LogInfo("Commanded WRITE_SINGLE_REGISTER with a new value - " + analogCommandingValue);
                }
                else if (pointItem.RegistarType == PointType.DIGITAL_OUTPUT && commandingValue is ushort discreteCommandingValue)
                {
                    modbusWriteCommandParams = new ModbusWriteCommandParameters(length,
                                                                           (byte)ModbusFunctionCode.WRITE_SINGLE_COIL,
                                                                           pointItem.Address,
                                                                           discreteCommandingValue);

                    Logger.LogInfo("Commanded WRITE_SINGLE_COIL with a new value - " + discreteCommandingValue);
                }
                else
                {
                    success = false;
                    modbusWriteCommandParams = null;
                    string message = $"Commanding arguments are not valid.";
                    Logger.LogError(message);
                }

                if(modbusWriteCommandParams != null)
                {
                    ModbusFunction modbusFunction = FunctionFactory.CreateModbusFunction(modbusWriteCommandParams);
                    success = functionExecutor.EnqueueCommand(modbusFunction);
                }

                //TOOD: alarming
                //bool AlarmChanged = CI.SetAlarms();
                //if (AlarmChanged)
                //{
                //    Logger.LogInfo("Alarm for item " + CI.Gid + " is set to " + CI.Alarm.ToString());
                //}

            }
            catch (Exception e)
            {
                success = false;
                string message = $"Exception in SendCommand() method.";
                Logger.LogError(message, e);
            }

            return success;
        }
    }
}
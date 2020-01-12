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

        public void SendAnalogCommand(long gid, float commandingValue)
        {
            if (scadaModel.CurrentScadaModel.TryGetValue(gid, out ISCADAModelPointItem pointItem))
            {
                if (pointItem.RegistarType == PointType.ANALOG_OUTPUT)
                {
                    ushort modbusValue = (ushort)commandingValue; //TODO: EGU convertion...
                    SendCommand(pointItem, modbusValue);
                }
                else
                {
                    string message = $"RegistarType of entity with gid: 0x{gid:X16} is not ANALOG_OUTPUT.";
                    Logger.LogError(message);
                    return;
                }
            }
            else
            {
                string message = $"Entity with gid: 0x{gid:X16} does not exist in current SCADA model.";
                Logger.LogError(message);
            }
        }

        public void SendDiscreteCommand(long gid, ushort commandingValue)
        {
            if (scadaModel.CurrentScadaModel.TryGetValue(gid, out ISCADAModelPointItem pointItem))
            {
                if (pointItem.RegistarType == PointType.DIGITAL_OUTPUT)
                {
                    SendCommand(pointItem, commandingValue);
                }
                else
                {
                    string message = $"RegistarType of entity with gid: 0x{gid:X16} is not DIGITAL_OUTPUT.";
                    Logger.LogError(message);
                    return;
                }
            }
            else
            {
                string message = $"Entity with gid: 0x{gid:X16} does not exist in current SCADA model.";
                Logger.LogError(message);
            }
        }

        private void SendCommand(ISCADAModelPointItem pointItem, object commandingValue)
        {
            ushort length = 6;
            ModbusWriteCommandParameters mdb_write_comm_pars;

            if (pointItem.RegistarType == PointType.ANALOG_OUTPUT && commandingValue is float analogCommandingValue)
            {
                mdb_write_comm_pars = new ModbusWriteCommandParameters(length,
                                                                       (byte)ModbusFunctionCode.WRITE_SINGLE_REGISTER,
                                                                       pointItem.Address,
                                                                       (ushort)analogCommandingValue);

                Logger.LogInfo("Commanded WRITE_SINGLE_REGISTER with a new value - " + analogCommandingValue);
            }
            else if (pointItem.RegistarType == PointType.DIGITAL_OUTPUT && commandingValue is ushort discreteCommandingValue)
            {
                mdb_write_comm_pars = new ModbusWriteCommandParameters(length,
                                                                       (byte)ModbusFunctionCode.WRITE_SINGLE_COIL,
                                                                       pointItem.Address,
                                                                       discreteCommandingValue);

                Logger.LogInfo("Commanded WRITE_SINGLE_COIL with a new value - " + discreteCommandingValue);
            }
            else
            {
                throw new ArgumentException("Commanding arguments are not valid.");
            }

            ModbusFunction modbusFunction = FunctionFactory.CreateModbusFunction(mdb_write_comm_pars);
            functionExecutor.EnqueueCommand(modbusFunction);

            //TOOD: alarming
            //bool AlarmChanged = CI.SetAlarms();
            //if (AlarmChanged)
            //{
            //    Logger.LogInfo("Alarm for item " + CI.Gid + " is set to " + CI.Alarm.ToString());
            //}
        }
    }
}
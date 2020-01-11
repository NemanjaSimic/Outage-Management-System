using Outage.Common;
using Outage.Common.ServiceContracts;
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

        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        private FunctionExecutor functionExecutor = FunctionExecutor.Instance;
        private SCADAModel scadaModel = SCADAModel.Instance;

        public void RecvCommand(long gid, object value)
        {
            if (scadaModel.CurrentScadaModel.TryGetValue(gid, out ISCADAModelPointItem pointItem))
            {
                ModbusWriteCommandParameters mdb_write_comm_pars;
                ushort length = 6;
                ushort commandedValue = (ushort)value;

                if (pointItem.RegistarType == PointType.ANALOG_OUTPUT)
                {
                    mdb_write_comm_pars = new ModbusWriteCommandParameters(length,
                                                                           (byte)ModbusFunctionCode.WRITE_SINGLE_REGISTER,
                                                                           pointItem.Address,
                                                                           commandedValue);

                    Logger.LogInfo("Commanded WRITE_SINGLE_REGISTER with a new value - " + commandedValue);
                }
                else if (pointItem.RegistarType == PointType.DIGITAL_OUTPUT)
                {
                    mdb_write_comm_pars = new ModbusWriteCommandParameters(length,
                                                                           (byte)ModbusFunctionCode.WRITE_SINGLE_COIL,
                                                                           pointItem.Address,
                                                                           commandedValue);

                    Logger.LogInfo("Commanded WRITE_SINGLE_COIL with a new value - " + commandedValue);
                }
                else
                {
                    string message = $"RegistarType of entity with gid: 0x{gid:X16} is nether ANALOG_OUTPUT nor DIGITAL_OUTPUT.";
                    Logger.LogError(message);
                    return;
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
            else
            {
                string message = $"Entity with gid: 0x{gid:X16} does not exist in current SCADA model.";
                Logger.LogError(message);
            }
        }
    }
}
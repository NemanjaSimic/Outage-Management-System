using Outage.Common.ServiceContracts;
using Outage.SCADA.ModBus;
using Outage.SCADA.ModBus.Connection;
using Outage.SCADA.ModBus.FunctionParameters;
using Outage.SCADA.ModBus.ModbusFuntions;
using Outage.SCADA.SCADA_Common;
using Outage.SCADA.SCADA_Config_Data.Configuration;
using Outage.SCADA.SCADA_Config_Data.Repository;
using System;

namespace Outage.SCADA.SCADAService.Command
{
    public class CommandService : ISCADACommand
    {
        private static FunctionExecutor fe = new FunctionExecutor(DataModelRepository.Instance.TcpPort);

        public CommandService()
        {
        }

        public void RecvCommand(long gid, object value)
        {
            if (DataModelRepository.Instance.Points.TryGetValue(gid, out ConfigItem CI))
            {
                if (CI.RegistarType == PointType.ANALOG_OUTPUT || CI.RegistarType == PointType.DIGITAL_OUTPUT)
                {
                    ModbusWriteCommandParameters mdb_write_comm_pars = null;

                    ushort CommandedValue;

                    if (CI.RegistarType == PointType.ANALOG_OUTPUT)
                    {
                        CommandedValue = (ushort)value;

                        mdb_write_comm_pars = new ModbusWriteCommandParameters
                       (6, (byte)ModbusFunctionCode.WRITE_SINGLE_REGISTER, CI.Address, CommandedValue);
                    }
                    else if (CI.RegistarType == PointType.DIGITAL_OUTPUT)
                    {
                        //TREBA BOOL ZBOG DIGITAL OUTPUT-a
                        CommandedValue = (ushort)value;

                        mdb_write_comm_pars = new ModbusWriteCommandParameters
                        (6, (byte)ModbusFunctionCode.WRITE_SINGLE_COIL, CI.Address, CommandedValue);
                    }

                    ModbusFunction fn = FunctionFactory.CreateModbusFunction(mdb_write_comm_pars);
                    fe.EnqueueCommand(fn);

                    CI.SetAlarms();
                }
            }
            else
            {
                Console.WriteLine("Ne postoji Point sa gidom " + gid);
            }
        }
    }
}
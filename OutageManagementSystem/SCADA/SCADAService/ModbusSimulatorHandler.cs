using Outage.Common;
using Outage.SCADA.SCADA_Config_Data.Repository;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Outage.SCADA.SCADAService
{
    public static class ModbusSimulatorHandler
    {
        public static void StartModbusSimulator()
        {
            try
            {
                DataModelRepository repo = DataModelRepository.Instance;
                Process.Start(repo.MdbSimExePath);

            }
            catch (Exception e)
            {
                LoggerWrapper.Instance.LogWarn("Exception on starting modbus simulator.", e);
            }
        }

        public static void StopModbusSimulaotrs()
        {
            try
            {
                DataModelRepository repo = DataModelRepository.Instance;
                Process[] modbusSimulators = Process.GetProcessesByName(repo.MdbSimExeName);

                foreach (Process mdbSim in modbusSimulators)
                {
                    mdbSim.Kill();
                }
            }
            catch (Exception e)
            {
                LoggerWrapper.Instance.LogWarn("Exception on stoping modbus simulators.", e);
            }
        }

        public static void RestartSimulator()
        {
            StopModbusSimulaotrs();
            StartModbusSimulator();
        }
    }
}

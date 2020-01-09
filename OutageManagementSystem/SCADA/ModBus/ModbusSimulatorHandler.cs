using Outage.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Outage.SCADA.SCADAConfigData;

namespace Outage.SCADA.ModBus
{
    public static class ModbusSimulatorHandler
    {
        public static void StartModbusSimulator()
        {
            try
            {
                SCADAConfigData.Configuration.SCADAConfigData repo = SCADAConfigData.Configuration.SCADAConfigData.Instance;
             
                Process process = new Process();
                process.StartInfo.FileName = repo.MdbSimExePath;
                process.Start();
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
                SCADAConfigData.Configuration.SCADAConfigData repo = SCADAConfigData.Configuration.SCADAConfigData.Instance;
                Process[] modbusSimulators = Process.GetProcessesByName(repo.MdbSimExeName.Replace(".exe", ""));

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

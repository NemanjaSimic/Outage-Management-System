using Outage.Common;
using Outage.SCADA.SCADACommon;
using Outage.SCADA.SCADAData.Configuration;
using System;
using System.Diagnostics;

namespace Outage.SCADA.ModBus
{
    public static class ModbusSimulatorHandler
    {
        public static void StartModbusSimulator()
        {
            try
            {
                ISCADAConfigData config = SCADAConfigData.Instance;

                Process process = new Process();
                process.StartInfo.FileName = config.ModbusSimulatorExePath;
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
                ISCADAConfigData config = SCADAConfigData.Instance;
                Process[] modbusSimulators = Process.GetProcessesByName(config.ModbusSimulatorExeName.Replace(".exe", ""));

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
using OMS.Common.ScadaContracts.Data;
using Outage.Common;
using System;
using System.Configuration;
using System.Net;

namespace OMS.Cloud.SCADA.ModelProviderService.Configuration
{
    internal static class ScadaConfigDataHelper
    {
        public static void ImportAppSettings(ScadaConfigData data)
        {
            ILogger logger = LoggerWrapper.Instance;

            if (ConfigurationManager.AppSettings["TcpPort"] is string tcpPortSetting)
            {
                if (ushort.TryParse(tcpPortSetting, out ushort tcpPort))
                {
                    data.TcpPort = tcpPort;
                }
                else
                {
                    string message = "TcpPort in SCADA configuration is either not defined or not valid.";
                    logger.LogError(message);
                    throw new Exception(message);
                }
            }

            if (ConfigurationManager.AppSettings["IpAddress"] is string ipAddressString)
            {
                if (IPAddress.TryParse(ipAddressString, out IPAddress ipAddress))
                {
                    data.IpAddress = ipAddress;
                }
                else
                {
                    string message = "IpAddress in SCADA configuration is either not defined or not valid.";
                    logger.LogError(message);
                    throw new Exception(message);
                }
            }

            if (ConfigurationManager.AppSettings["UnitAddress"] is string unitAddressSetting)
            {
                if (byte.TryParse(unitAddressSetting, out byte unitAddress))
                {
                    data.UnitAddress = unitAddress;
                }
                else
                {
                    string message = "UnitAddress in SCADA configuration is either not defined or not valid.";
                    logger.LogError(message);
                    throw new Exception(message);
                }
            }

            if (ConfigurationManager.AppSettings["AcquisitionInterval"] is string acquisitionIntervalSetting)
            {
                if (ushort.TryParse(acquisitionIntervalSetting, out ushort interval))
                {
                    data.AcquisitionInterval = interval;
                }
                else
                {
                    data.AcquisitionInterval = 10000;
                    logger.LogWarn("AcquisitionInterval in SCADA configuration is either not defined or not valid.");
                }
            }

            if (ConfigurationManager.AppSettings["FunctionExecutionInterval"] is string functionExecutionIntervalSetting)
            {
                if (ushort.TryParse(functionExecutionIntervalSetting, out ushort interval))
                {
                    data.FunctionExecutionInterval = interval;
                }
                else
                {
                    data.FunctionExecutionInterval = 10000;
                    logger.LogWarn("FunctionExecutionInterval in SCADA configuration is either not defined or not valid.");
                }
            }

            if (ConfigurationManager.AppSettings["ModbusSimulatorExeName"] is string mdbSimExeName)
            {
                data.ModbusSimulatorExeName = mdbSimExeName;

                if (ConfigurationManager.AppSettings["ModbusSimulatorExePath"] is string mdbSimExePath)
                {
                    data.ModbusSimulatorExePath = Environment.CurrentDirectory.Replace(@"\SCADAServiceHost\bin\Debug", $@"{mdbSimExePath}\{data.ModbusSimulatorExeName}");
                }
            }
        }
    }
}

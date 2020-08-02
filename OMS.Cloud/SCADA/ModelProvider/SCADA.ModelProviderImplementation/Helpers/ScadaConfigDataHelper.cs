using OMS.Common.Cloud.Logger;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts.DataContracts;
using System;
using System.Configuration;
using System.Net;

namespace SCADA.ModelProviderImplementation.Helpers
{
    internal class ScadaConfigDataHelper
    {
        private static readonly object lockSync = new object();
        private static IScadaConfigData scadaConfigData;

        public static IScadaConfigData GetScadaConfigData()
        {
            if (scadaConfigData == null)
            {
                lock (lockSync)
                {
                    if (scadaConfigData == null)
                    {
                        scadaConfigData = ImportAppSettings();
                    }
                }
            }

            return scadaConfigData;
        }

        private static ICloudLogger logger;
        private static ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        private static IScadaConfigData ImportAppSettings()
        {
            string baseLogString = $"{typeof(ScadaConfigDataHelper)} [static] =>";

            ScadaConfigData data = new ScadaConfigData();

            if (ConfigurationManager.AppSettings["TcpPort"] is string tcpPortSetting)
            {
                if (ushort.TryParse(tcpPortSetting, out ushort tcpPort))
                {
                    data.TcpPort = tcpPort;
                }
                else
                {
                    string errorMessage = $"{baseLogString} ImportAppSettings => TcpPort in SCADA configuration is either not defined or not valid.";
                    Logger.LogError(errorMessage);
                    throw new Exception(errorMessage);
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
                    string errorMessage = $"{baseLogString} ImportAppSettings => IpAddress in SCADA configuration is either not defined or not valid.";
                    Logger.LogError(errorMessage);
                    throw new Exception(errorMessage);
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
                    string errorMessage = $"{baseLogString} ImportAppSettings => UnitAddress in SCADA configuration is either not defined or not valid.";
                    Logger.LogError(errorMessage);
                    throw new Exception(errorMessage);
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
                    string warnMessage = $"{baseLogString} ImportAppSettings => AcquisitionInterval in SCADA configuration is either not defined or not valid.";
                    Logger.LogWarning(warnMessage);
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
                    string warnMessage = $"{baseLogString} ImportAppSettings => FunctionExecutionInterval in SCADA configuration is either not defined or not valid.";
                    Logger.LogWarning(warnMessage);
                }
            }

            if (ConfigurationManager.AppSettings["ModbusSimulatorExeName"] is string mdbSimExeName)
            {
                data.ModbusSimulatorExeName = mdbSimExeName;

                if (ConfigurationManager.AppSettings["ModbusSimulatorExePath"] is string mdbSimExePath)
                {
                    data.ModbusSimulatorExePath = Environment.CurrentDirectory.Replace(@"\SCADAServiceHost\bin\Debug", $@"{mdbSimExePath}\{data.ModbusSimulatorExeName}");
                }
                else
                {
                    string warnMessage = $"{baseLogString} ImportAppSettings => ModbusSimulatorExePath in SCADA configuration is either not defined or not valid.";
                    Logger.LogWarning(warnMessage);
                }
            }
            else
            {
                string warnMessage = $"{baseLogString} ImportAppSettings => ModbusSimulatorExeName in SCADA configuration is either not defined or not valid.";
                Logger.LogWarning(warnMessage);
            }

            string infoMessage = $"{baseLogString} ImportAppSettings => Scada config data Imported.";
            Logger.LogInformation(infoMessage);

            string debugMessage = $"{baseLogString} ImportAppSettings => AcquisitionInterval: {data.AcquisitionInterval}, FunctionExecutionInterval: {data.FunctionExecutionInterval}, IpAddress: [{data.IpAddress}, ModbusSimulatorExeName: {data.ModbusSimulatorExeName}, ModbusSimulatorExePath: {data.ModbusSimulatorExePath}, TcpPort: {data.TcpPort}, UnitAddress: {data.UnitAddress}.";
            Logger.LogDebug(debugMessage);

            return data;
        }
    }
}

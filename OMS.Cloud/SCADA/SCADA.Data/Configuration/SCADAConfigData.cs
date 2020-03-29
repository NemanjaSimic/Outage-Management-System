using Outage.Common;
using System;
using System.Configuration;
using System.Net;
using OMS.Common.SCADA;

namespace OMS.Cloud.SCADA.Data.Configuration
{
    public class SCADAConfigData : ISCADAConfigData
    {
        private ILogger logger;

        private ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }


        public ushort TcpPort { get; private set; }
        public IPAddress IpAddress { get; private set; }
        public byte UnitAddress { get; private set; }
        public ushort AcquisitionInterval { get; private set; }
        public ushort FunctionExecutionInterval { get; private set; }

        public string ModbusSimulatorExeName { get; private set; } = string.Empty;
        public string ModbusSimulatorExePath { get; private set; } = string.Empty;

        #region Instance

        private static SCADAConfigData _instance;

        public static SCADAConfigData Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SCADAConfigData();
                }

                return _instance;
            }
        }
        #endregion Instance

        private SCADAConfigData()
        {
            ImportAppSettings();
        }

        private void ImportAppSettings()
        {
            if (ConfigurationManager.AppSettings["TcpPort"] is string tcpPortSetting)
            {
                if (ushort.TryParse(tcpPortSetting, out ushort tcpPort))
                {
                    TcpPort = tcpPort;
                }
                else
                {
                    string message = "TcpPort in SCADA configuration is either not defined or not valid.";
                    Logger.LogError(message);
                    throw new Exception(message);
                }
            }

            if (ConfigurationManager.AppSettings["IpAddress"] is string ipAddressString)
            {
                if(IPAddress.TryParse(ipAddressString, out IPAddress ipAddress))
                {
                    IpAddress = ipAddress;
                }
                else
                {
                    string message = "IpAddress in SCADA configuration is either not defined or not valid.";
                    Logger.LogError(message);
                    throw new Exception(message);
                }
            }

            if (ConfigurationManager.AppSettings["UnitAddress"] is string unitAddressSetting)
            {
                if (byte.TryParse(unitAddressSetting, out byte unitAddress))
                {
                    UnitAddress = unitAddress;
                }
                else
                {
                    string message = "UnitAddress in SCADA configuration is either not defined or not valid.";
                    Logger.LogError(message);
                    throw new Exception(message);
                }
            }

            if (ConfigurationManager.AppSettings["AcquisitionInterval"] is string acquisitionIntervalSetting)
            {
                if (ushort.TryParse(acquisitionIntervalSetting, out ushort interval))
                {
                    AcquisitionInterval = interval;
                }
                else
                {
                    AcquisitionInterval = 10000;
                    Logger.LogWarn("AcquisitionInterval in SCADA configuration is either not defined or not valid.");
                }
            }

            if (ConfigurationManager.AppSettings["FunctionExecutionInterval"] is string functionExecutionIntervalSetting)
            {
                if (ushort.TryParse(functionExecutionIntervalSetting, out ushort interval))
                {
                    FunctionExecutionInterval = interval;
                }
                else
                {
                    FunctionExecutionInterval = 10000;
                    Logger.LogWarn("FunctionExecutionInterval in SCADA configuration is either not defined or not valid.");
                }
            }

            if (ConfigurationManager.AppSettings["ModbusSimulatorExeName"] is string mdbSimExeName)
            {
                ModbusSimulatorExeName = mdbSimExeName;

                if (ConfigurationManager.AppSettings["ModbusSimulatorExePath"] is string mdbSimExePath)
                {
                    ModbusSimulatorExePath = Environment.CurrentDirectory.Replace(@"\SCADAServiceHost\bin\Debug", $@"{mdbSimExePath}\{ModbusSimulatorExeName}");
                }
            }
        }
    }
}
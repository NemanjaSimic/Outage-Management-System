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

        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }


        public ushort TcpPort { get; protected set; }
        public IPAddress IpAddress { get; protected set; }
        public byte UnitAddress { get; protected set; }
        public ushort Interval { get; protected set; }

        public string ModbusSimulatorExeName { get; protected set; } = string.Empty;
        public string ModbusSimulatorExePath { get; protected set; } = string.Empty;

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

            if (ConfigurationManager.AppSettings["Interval"] is string intervalSetting)
            {
                if (ushort.TryParse(intervalSetting, out ushort interval))
                {
                    Interval = interval;
                }
                else
                {
                    Interval = 10000;
                    Logger.LogWarn("Interval in SCADA configuration is either not defined or not valid.");
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
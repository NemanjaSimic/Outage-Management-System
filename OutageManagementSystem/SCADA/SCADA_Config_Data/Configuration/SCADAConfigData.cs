using Outage.Common;
using Outage.SCADA.SCADACommon;
using System;
using System.Configuration;

namespace Outage.SCADA.SCADAData.Configuration
{
    public class SCADAConfigData : ISCADAConfigData
    {
        private ILogger logger = LoggerWrapper.Instance;

        public ushort TcpPort { get; protected set; }
        public string IpAddress { get; protected set; }
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
                    logger.LogError(message);
                    throw new Exception(message);
                }
            }

            if (ConfigurationManager.AppSettings["IpAddress"] is string ipAddress)
            {
                //TOOD: is valid ip address? => error
                IpAddress = ipAddress;
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
                    logger.LogError(message);
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
                    logger.LogWarn("Interval in SCADA configuration is either not defined or not valid.");
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
using Outage.Common;
using System;
using System.Configuration;

namespace Outage.SCADA.SCADAConfigData.Configuration
{
    public class SCADAConfigData
    {
        private ILogger logger = LoggerWrapper.Instance;

        public ushort TcpPort { get; protected set; }
        public byte UnitAddress { get; protected set; }
        public ushort Interval { get; protected set; }

        public string MdbSimExeName { get; protected set; } = string.Empty;
        public string MdbSimExePath { get; protected set; } = string.Empty;

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
        #endregion

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
                    //TODO: err log
                    throw new Exception("TcpPort in SCADA configuration is either not defined or not valid.");
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
                    //TODO: err log
                    throw new Exception("UnitAddress in SCADA configuration is either not defined or not valid.");
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
                    //TODO: warnnig log
                    //throw new Exception("Interval in SCADA configuration is either not defined or not valid.");
                }
            }

            //if (ConfigurationManager.AppSettings["ConfigFileName"] is string configFileName)
            //{
            //    ConfigFileName = configFileName;

            //    if (ConfigurationManager.AppSettings["CurrentConfigPath"] is string currentConfigPath)
            //    {
            //        CurrentConfigPath = Environment.CurrentDirectory.Replace(@"\SCADAServiceHost\bin\Debug", $@"{currentConfigPath}\{ConfigFileName}");
            //    }

            //    if (ConfigurationManager.AppSettings["BackupConfigPath"] is string backupConfigPath)
            //    {
            //        BackupConfigPath = Environment.CurrentDirectory.Replace(@"\SCADAServiceHost\bin\Debug", $@"{backupConfigPath}\{ConfigFileName}");
            //    }
            //}

            if (ConfigurationManager.AppSettings["MdbSimExeName"] is string mdbSimExeName)
            {
                MdbSimExeName = mdbSimExeName;

                if (ConfigurationManager.AppSettings["MdbSimExePath"] is string mdbSimExePath)
                {
                    MdbSimExePath = Environment.CurrentDirectory.Replace(@"\SCADAServiceHost\bin\Debug", $@"{mdbSimExePath}\{MdbSimExeName}");
                }   
            }

        }
    }
}
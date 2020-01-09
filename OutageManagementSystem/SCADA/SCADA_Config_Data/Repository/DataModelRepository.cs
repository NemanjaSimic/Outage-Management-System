﻿using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.ServiceContracts;
using Outage.Common.ServiceProxies;
using Outage.SCADA.SCADA_Common;
using Outage.SCADA.SCADA_Config_Data.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceModel;
using System.Threading;

namespace Outage.SCADA.SCADA_Config_Data.Repository
{
    public class DataModelRepository
    {
        private ModelResourcesDesc resourcesDesc = new ModelResourcesDesc();
        private ILogger logger = LoggerWrapper.Instance;

        #region Proxies
        private NetworkModelGDAProxy gdaQueryProxy = null;

        protected NetworkModelGDAProxy GdaQueryProxy
        {
            get
            {
                int numberOfTries = 0;

                while (numberOfTries < 10)
                {
                    try
                    {
                        if (gdaQueryProxy != null)
                        {
                            gdaQueryProxy.Abort();
                            gdaQueryProxy = null;
                        }

                        gdaQueryProxy = new NetworkModelGDAProxy(EndpointNames.NetworkModelGDAEndpoint);
                        gdaQueryProxy.Open();
                        break;
                    }
                    catch (Exception ex)
                    {
                        string message = $"Exception on NetworkModelGDAProxy initialization. Message: {ex.Message}";
                        logger.LogError(message, ex);
                        gdaQueryProxy = null;
                    }
                    finally
                    {
                        numberOfTries++;
                        logger.LogDebug($"DataModelRepository: GdaQueryProxy getter, try number: {numberOfTries}.");
                        Thread.Sleep(500);
                    }
                }

                return gdaQueryProxy;
            }
        }
        #endregion

        public ushort TcpPort { get; protected set; }
        public byte UnitAddress { get; protected set; }
        public ushort Interval { get; protected set; }

        public string ConfigFileName { get; protected set; } = string.Empty;
        public string CurrentConfigPath { get; protected set; } = string.Empty;
        public string BackupConfigPath { get; protected set; } = string.Empty;
        public string MdbSimExeName { get; protected set; } = string.Empty;
        public string MdbSimExePath { get; protected set; } = string.Empty;

        public Dictionary<long, ConfigItem> Points { get; set; }
        public Dictionary<long, ResourceDescription> NetworkModel { get; protected set; }
        public Dictionary<long, Dictionary<ModelCode, Property>> NetworkModelProps { get; protected set; }

        #region Instance
        private static DataModelRepository _instance;

        public static DataModelRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DataModelRepository();
                }

                return _instance;
            }
        }
        #endregion

        private DataModelRepository()
        {
            Points = new Dictionary<long, ConfigItem>();
            NetworkModel = new Dictionary<long, ResourceDescription>();
            NetworkModelProps = new Dictionary<long, Dictionary<ModelCode, Property>>();

            ImportAppSettings();
        }

        public bool ImportModel()
        {
            //TODO: log info
            Console.WriteLine("Importing analog measurements started...");
            bool analogImportSuccess = ImportAnalog();
            //TODO: log info finish
            Console.WriteLine($"Importing analog measurements finished. ['success' value: {analogImportSuccess}]");

            //TODO: log info
            Console.WriteLine("Importing discrete measurements started...");
            bool discreteImportSuccess = ImportDiscrete();
            //TODO: log info finish
            Console.WriteLine($"Importing discrete measurements finished. ['success' value: {discreteImportSuccess}]");

            return analogImportSuccess && discreteImportSuccess;
        }

        private bool ImportAnalog()
        {
            bool success;
            int numberOfResources = 1000;
            List<ModelCode> props = resourcesDesc.GetAllPropertyIds(ModelCode.ANALOG);

            try
            {
                using (NetworkModelGDAProxy gdaProxy = GdaQueryProxy)
                {
                    if(gdaProxy != null)
                    {
                        int iteratorId = gdaProxy.GetExtentValues(ModelCode.ANALOG, props);
                        int resourcesLeft = gdaProxy.IteratorResourcesLeft(iteratorId);

                        while (resourcesLeft > 0)
                        {
                            List<ResourceDescription> rds = gdaProxy.IteratorNext(numberOfResources, iteratorId);
                            for (int i = 0; i < rds.Count; i++)
                            {
                                if (rds[i] != null)
                                {
                                    NetworkModel.Add(rds[i].Id, rds[i]);
                                    Points.Add(rds[i].Id, ConfigurateConfigItem(rds[i].Properties, ModelCode.ANALOG));
                                    //TODO: log debug
                                }
                            }
                            resourcesLeft = gdaProxy.IteratorResourcesLeft(iteratorId);
                        }

                        success = true;
                    }
                    else
                    { 
                        success = false;
                        string errMsg = "From ImportAnalog() method: NetworkModelGDAProxy is null.";
                        logger.LogWarn(errMsg);
                    }
                }
            }
            catch (Exception ex)
            {
                success = false;
                string errorMessage = $"ImportAnalog failed with error: {ex.Message}";
                Console.WriteLine(errorMessage);
                logger.LogError(errorMessage, ex);
            }

            return success;
        }

        private bool ImportDiscrete()
        {
            bool success;
            int numberOfResources = 100;
            List<ModelCode> props = resourcesDesc.GetAllPropertyIds(ModelCode.DISCRETE);

            try
            {
                using (NetworkModelGDAProxy gdaProxy = GdaQueryProxy)
                {
                    if(gdaProxy != null)
                    {
                        int iteratorId = gdaProxy.GetExtentValues(ModelCode.DISCRETE, props);
                        int resourcesLeft = gdaProxy.IteratorResourcesLeft(iteratorId);

                        while (resourcesLeft > 0)
                        {
                            List<ResourceDescription> rds = gdaProxy.IteratorNext(numberOfResources, iteratorId);
                            for (int i = 0; i < rds.Count; i++)
                            {
                                if (rds[i] != null)
                                {
                                    NetworkModel.Add(rds[i].Id, rds[i]);
                                    Points.Add(rds[i].Id, ConfigurateConfigItem(rds[i].Properties, ModelCode.DISCRETE));
                                    //TODO: log debug
                                }
                            }
                            resourcesLeft = gdaProxy.IteratorResourcesLeft(iteratorId);
                        }

                        success = true;
                    }
                    else
                    {
                        success = false;
                        string errMsg = "From ImportDiscrete() method: NetworkModelGDAProxy is null.";
                        logger.LogWarn(errMsg);
                    }
                }
            }
            catch (Exception ex)
            {
                success = false;
                string errorMessage = $"ImportDiscrete failed with error: {ex.Message}";
                Console.WriteLine(errorMessage);
                logger.LogError(errorMessage, ex);
            }

            return success;
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

            if (ConfigurationManager.AppSettings["ConfigFileName"] is string configFileName)
            {
                ConfigFileName = configFileName;

                if (ConfigurationManager.AppSettings["CurrentConfigPath"] is string currentConfigPath)
                {
                    CurrentConfigPath = Environment.CurrentDirectory.Replace(@"\SCADAServiceHost\bin\Debug", $@"{currentConfigPath}\{ConfigFileName}");
                }

                if (ConfigurationManager.AppSettings["BackupConfigPath"] is string backupConfigPath)
                {
                    BackupConfigPath = Environment.CurrentDirectory.Replace(@"\SCADAServiceHost\bin\Debug", $@"{backupConfigPath}\{ConfigFileName}");
                }
            }

            if (ConfigurationManager.AppSettings["MdbSimExeName"] is string mdbSimExeName)
            {
                MdbSimExeName = mdbSimExeName;

                if (ConfigurationManager.AppSettings["MdbSimExePath"] is string mdbSimExePath)
                {
                    MdbSimExePath = Environment.CurrentDirectory.Replace(@"\SCADAServiceHost\bin\Debug", $@"{mdbSimExePath}\{MdbSimExeName}");
                }   
            }

        }

        public ConfigItem ConfigurateConfigItem(List<Property> props, ModelCode type)
        {
            ConfigItem configItem = new ConfigItem();
            long gid = 0;
            Dictionary<ModelCode, Property> propDictionary = new Dictionary<ModelCode, Property>();
            foreach (var item in props)
            {
                switch (item.Id)
                {
                    case ModelCode.IDOBJ_GID:
                        gid = item.AsLong();
                        propDictionary.Add(item.Id, item);
                        configItem.Gid = gid;
                        break;

                    case ModelCode.IDOBJ_NAME:
                        propDictionary.Add(item.Id, item);
                        configItem.Name = item.AsString();
                        break;

                    case ModelCode.DISCRETE_CURRENTOPEN:
                        propDictionary.Add(item.Id, item);
                        configItem.CurrentValue = (item.AsBool() == true) ? 1 : 0;
                        break;

                    case ModelCode.DISCRETE_MAXVALUE:
                        propDictionary.Add(item.Id, item);
                        configItem.MaxValue = item.AsInt();
                        break;

                    case ModelCode.DISCRETE_MINVALUE:
                        propDictionary.Add(item.Id, item);
                        configItem.MinValue = item.AsInt();
                        break;

                    case ModelCode.DISCRETE_NORMALVALUE:
                        propDictionary.Add(item.Id, item);
                        configItem.DefaultValue = item.AsInt();
                        break;

                    case ModelCode.MEASUREMENT_ADDRESS:
                        if (ushort.TryParse(item.AsString(), out ushort address))
                        {
                            configItem.Address = address;
                        }
                        else
                        {
                            //TODO: log err address is either not defined or is invalid
                            //todo: exception?
                        }
                        break;

                    case ModelCode.MEASUREMENT_ISINPUT:
                        propDictionary.Add(item.Id, item);
                        if (type == ModelCode.ANALOG)
                        {
                            configItem.RegistarType = (item.AsBool() == true) ? PointType.ANALOG_INPUT : PointType.ANALOG_OUTPUT;
                        }
                        else if(type == ModelCode.DISCRETE)
                        {
                            configItem.RegistarType = (item.AsBool() == true) ? PointType.DIGITAL_INPUT : PointType.DIGITAL_OUTPUT;
                        }
                        else
                        {
                            //TODO: log err
                            //todo: exception?
                        }
                        break;

                    case ModelCode.ANALOG_CURRENTVALUE:
                        propDictionary.Add(item.Id, item);
                        configItem.CurrentValue = item.AsFloat();
                        break;

                    case ModelCode.ANALOG_MAXVALUE:
                        propDictionary.Add(item.Id, item);
                        configItem.MaxValue = item.AsFloat();
                        break;

                    case ModelCode.ANALOG_MINVALUE:
                        propDictionary.Add(item.Id, item);
                        configItem.MinValue = item.AsFloat();
                        break;

                    case ModelCode.ANALOG_NORMALVALUE:
                        propDictionary.Add(item.Id, item);
                        configItem.DefaultValue = item.AsFloat();
                        break;

                    default:
                        break;
                }

                if (configItem.RegistarType == PointType.ANALOG_INPUT || configItem.RegistarType == PointType.ANALOG_OUTPUT)
                {
                    configItem.LowLimit = configItem.EGU_Min + 200;
                    configItem.HighLimit = configItem.EGU_Max - 200;
                }
                else if(configItem.RegistarType == PointType.DIGITAL_INPUT || configItem.RegistarType == PointType.DIGITAL_OUTPUT)
                {
                    configItem.LowLimit = 0;
                    configItem.HighLimit = 1;
                }
            }

            if(NetworkModelProps.ContainsKey(gid))
            {
                NetworkModelProps[gid] = propDictionary;
            }
            else
            {
                NetworkModelProps.Add(gid, propDictionary);
            }

            return configItem;
        }
    }
}
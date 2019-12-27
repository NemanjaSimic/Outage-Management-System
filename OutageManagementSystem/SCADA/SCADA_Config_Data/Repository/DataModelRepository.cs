using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.ServiceContracts;
using Outage.Common.ServiceProxies;
using Outage.SCADA.SCADA_Common;
using Outage.SCADA.SCADA_Config_Data.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceModel;

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
                try
                {
                    if (gdaQueryProxy != null)
                    {
                        gdaQueryProxy.Abort();
                        gdaQueryProxy = null;
                    }

                    gdaQueryProxy = new NetworkModelGDAProxy(EndpointNames.NetworkModelGDAEndpoint);
                    gdaQueryProxy.Open();

                }
                catch (Exception ex)
                {
                    string message = $"Exception on NetworkModelGDAProxy initialization. Message: {ex.Message}";
                    logger.LogError(message, ex);
                    gdaQueryProxy = null;
                }
                        
                return gdaQueryProxy;
            }
        }
        #endregion

        public ushort TcpPort { get; protected set; }
        public byte UnitAddress { get; protected set; }
        public int Interval { get; protected set; }

        public Dictionary<long, ConfigItem> Points;
        public Dictionary<long, Dictionary<ModelCode, Property>> NMS_Model_Props;
        public Dictionary<long, ResourceDescription> NMS_Model;

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
            NMS_Model = new Dictionary<long, ResourceDescription>();
            NMS_Model_Props = new Dictionary<long, Dictionary<ModelCode, Property>>();
            
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
                    int iteratorId = gdaProxy.GetExtentValues(ModelCode.ANALOG, props);
                    int resourcesLeft = gdaProxy.IteratorResourcesLeft(iteratorId);

                    while (resourcesLeft > 0)
                    {
                        List<ResourceDescription> rds = gdaProxy.IteratorNext(numberOfResources, iteratorId);
                        for (int i = 0; i < rds.Count; i++)
                        {
                            if (rds[i] != null)
                            {
                                NMS_Model.Add(rds[i].Id, rds[i]);
                                Points.Add(rds[i].Id, ConfigurateConfigItem(rds[i].Properties, true));
                                //TODO: log debug
                            }
                        }
                        resourcesLeft = gdaProxy.IteratorResourcesLeft(iteratorId);
                    }
                }

                success = true;
            }
            catch (Exception ex)
            {
                //TODO: err log
                Console.WriteLine("ImportAnalog failed with error: {0}", ex.Message);
                success = false;
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
                    int iteratorId = gdaProxy.GetExtentValues(ModelCode.DISCRETE, props);
                    int resourcesLeft = gdaProxy.IteratorResourcesLeft(iteratorId);

                    while (resourcesLeft > 0)
                    {
                        List<ResourceDescription> rds = gdaProxy.IteratorNext(numberOfResources, iteratorId);
                        for (int i = 0; i < rds.Count; i++)
                        {
                            if (rds[i] != null)
                            {
                                NMS_Model.Add(rds[i].Id, rds[i]);
                                Points.Add(rds[i].Id, ConfigurateConfigItem(rds[i].Properties, true));
                                //TODO: log debug
                            }
                        }
                        resourcesLeft = gdaProxy.IteratorResourcesLeft(iteratorId);
                    }
                }

                success = true;
            }
            catch (Exception ex)
            {
                //TODO: err log
                Console.WriteLine("ImportDiscrete failed with error: {0}", ex.Message);
                success = false;
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
                if (int.TryParse(intervalSetting, out int interval))
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
        }

        private ConfigItem ConfigurateConfigItem(List<Property> props, bool isAna)
        {
            ConfigItem configItem = new ConfigItem();
            long gid = 0;
            Dictionary<ModelCode, Property> prop = new Dictionary<ModelCode, Property>();
            foreach (var item in props)
            {
                switch (item.Id)
                {
                    case ModelCode.IDOBJ_GID:
                        gid = item.AsLong();
                        prop.Add(item.Id, item);
                        configItem.Gid = gid;
                        break;

                    case ModelCode.IDOBJ_NAME:
                        prop.Add(item.Id, item);
                        configItem.Name = item.AsString();
                        break;

                    case ModelCode.DISCRETE_CURRENTOPEN:
                        prop.Add(item.Id, item);
                        configItem.CurrentValue = (item.AsBool() == true) ? 1 : 0;
                        break;

                    case ModelCode.DISCRETE_MAXVALUE:
                        prop.Add(item.Id, item);
                        configItem.MaxValue = item.AsInt();
                        break;

                    case ModelCode.DISCRETE_MINVALUE:
                        prop.Add(item.Id, item);
                        configItem.MinValue = item.AsInt();
                        break;

                    case ModelCode.DISCRETE_NORMALVALUE:
                        prop.Add(item.Id, item);
                        configItem.DefaultValue = item.AsInt();
                        break;

                    case ModelCode.MEASUREMENT_ADDRESS:
                        if(ushort.TryParse(item.AsString(), out ushort address))
                        {
                            configItem.Address = address;
                        }
                        else
                        {
                            //TODO: log err address is either not defined or is invalid
                        }
                        break;

                    case ModelCode.MEASUREMENT_ISINPUT:
                        prop.Add(item.Id, item);
                        if (isAna)
                        {
                            configItem.RegistarType = (item.AsBool() == true) ? PointType.ANALOG_INPUT : PointType.ANALOG_OUTPUT;
                        }
                        else
                        {
                            configItem.RegistarType = (item.AsBool() == true) ? PointType.DIGITAL_INPUT : PointType.DIGITAL_OUTPUT;
                        }
                        break;

                    case ModelCode.ANALOG_CURRENTVALUE:
                        prop.Add(item.Id, item);
                        configItem.CurrentValue = item.AsFloat();
                        break;

                    case ModelCode.ANALOG_MAXVALUE:
                        prop.Add(item.Id, item);
                        configItem.MaxValue = item.AsFloat();
                        break;

                    case ModelCode.ANALOG_MINVALUE:
                        prop.Add(item.Id, item);
                        configItem.MinValue = item.AsFloat();
                        break;

                    case ModelCode.ANALOG_NORMALVALUE:
                        prop.Add(item.Id, item);
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
                else
                {
                    configItem.LowLimit = 0;
                    configItem.HighLimit = 1;
                }
            }
            NMS_Model_Props.Add(gid, prop);
            return configItem;
        }
    }
}
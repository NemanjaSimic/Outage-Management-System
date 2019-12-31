using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.ServiceContracts;
using Outage.SCADA.SCADA_Common;
using Outage.SCADA.SCADA_Config_Data.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.ServiceModel;

namespace Outage.SCADA.SCADA_Config_Data.Repository
{
    public class DataModelRepository
    {
        public ushort TcpPort { get; protected set; }
        public byte UnitAddress { get; protected set; }
        public ushort Interval { get; protected set; }
        public string ConfigFileName { get; protected set; }
        //public FunctionExecutor functionExecutor { get; set; }
        public INetworkModelGDAContract gdaQueryProxy = null;
        public string pathToDeltaCfg = "";
        public string pathMdbSimCfg = "";
        public Dictionary<long, ConfigItem> Points;
        public Dictionary<long, ResourceDescription> NMS_Model;
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

        private DataModelRepository()
        {
            Points = new Dictionary<long, ConfigItem>();
            NMS_Model = new Dictionary<long, ResourceDescription>();
            TcpPort = ushort.Parse(ConfigurationManager.AppSettings["TcpPort"]);
            UnitAddress = byte.Parse(ConfigurationManager.AppSettings["UnitAddress"]);
            Interval = ushort.Parse(ConfigurationManager.AppSettings["Interval"]);
            ConfigFileName = ConfigurationManager.AppSettings["ConfigFileName"];
            pathToDeltaCfg = Environment.CurrentDirectory;
            pathToDeltaCfg = pathToDeltaCfg.Replace("\\SCADA\\bin\\Debug", $"\\MdbSimTest\\{ConfigFileName}");
            pathMdbSimCfg = Environment.CurrentDirectory.Replace("\\SCADA\\bin\\Debug", $"\\MdbSim\\{ConfigFileName}");
            try
            {
                gdaQueryProxy = new ChannelFactory<INetworkModelGDAContract>(EndpointNames.NetworkModelGDAEndpoint).CreateChannel();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        public bool ImportModel()
        {
            bool anaIm;
            bool disIm;
            Console.WriteLine("Importing analog...");
            anaIm = ImportAnalog();
            Console.WriteLine("Importing discrete...");
            disIm = ImportDiscrete();
            return anaIm && disIm;
        }

        private bool ImportAnalog()
        {
            int iteratorId = 0;
            int resourcesLeft = 0;
            int numberOfResources = 100;
            List<ModelCode> props = new List<ModelCode>
            {
                ModelCode.IDOBJ_GID,
                ModelCode.IDOBJ_NAME,
                ModelCode.MEASUREMENT_ADDRESS,
                ModelCode.MEASUREMENT_ISINPUT,
                ModelCode.ANALOG_CURRENTVALUE,
                ModelCode.ANALOG_MAXVALUE,
                ModelCode.ANALOG_MINVALUE,
                ModelCode.ANALOG_NORMALVALUE
            };
            try
            {
                iteratorId = gdaQueryProxy.GetExtentValues(ModelCode.ANALOG, props);
          
                resourcesLeft = gdaQueryProxy.IteratorResourcesLeft(iteratorId);
                while (resourcesLeft > 0)
                {
                    List<ResourceDescription> rds = gdaQueryProxy.IteratorNext(numberOfResources, iteratorId);
                    for (int i = 0; i < rds.Count; i++)
                    {
                        if (rds[i] != null)
                        {   
                            NMS_Model.Add(rds[i].Id, rds[i]);
                            Points.Add(rds[i].Id, ConfigurateConfigItem(rds[i].Properties, true));
                        }
                    }
                    resourcesLeft = gdaQueryProxy.IteratorResourcesLeft(iteratorId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ImportAnalog failed with error: {0}", ex.Message);
                return false;
            }
            return true;
        }

        private bool ImportDiscrete()
        {
            int iteratorId = 0;
            int resourcesLeft = 0;
            int numberOfResources = 100;
            List<ModelCode> props = new List<ModelCode>
            {
                ModelCode.IDOBJ_GID,
                ModelCode.IDOBJ_NAME,
                ModelCode.MEASUREMENT_ADDRESS,
                ModelCode.MEASUREMENT_ISINPUT,
                ModelCode.DISCRETE_CURRENTOPEN,
                ModelCode.DISCRETE_MAXVALUE,
                ModelCode.DISCRETE_MINVALUE,
                ModelCode.DISCRETE_NORMALVALUE
            };
            try
            {
                iteratorId = gdaQueryProxy.GetExtentValues(ModelCode.DISCRETE, props);
                resourcesLeft = gdaQueryProxy.IteratorResourcesLeft(iteratorId);
                
                while (resourcesLeft > 0)
                {
                    List<ResourceDescription> rds = gdaQueryProxy.IteratorNext(numberOfResources, iteratorId);
                    for (int i = 0; i < rds.Count; i++)
                    {
                        if (rds[i] != null)
                        {  
                            NMS_Model.Add(rds[i].Id, rds[i]);
                            Points.Add(rds[i].Id, ConfigurateConfigItem(rds[i].Properties, false));
                        }
                    }
                    resourcesLeft = gdaQueryProxy.IteratorResourcesLeft(iteratorId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ImportDiscrete failed with error: {0}", ex.Message);
                return false;
            }
            return true;
        }

        public ConfigItem ConfigurateConfigItem(List<Property> props, bool isAna)
        {
            ConfigItem configItem = new ConfigItem();
            long gid = 0;
            foreach (var item in props)
            {
                switch (item.Id)
                {
                    case ModelCode.IDOBJ_GID:
                        gid = item.AsLong();
                        configItem.Gid = gid;
                        break;

                    case ModelCode.IDOBJ_NAME:
                        configItem.Name = item.AsString();
                        break;

                    case ModelCode.DISCRETE_CURRENTOPEN:
                        configItem.CurrentValue = (item.AsBool() == true) ? 1 : 0;
                        break;

                    case ModelCode.DISCRETE_MAXVALUE:
                        configItem.MaxValue = item.AsInt();
                        break;

                    case ModelCode.DISCRETE_MINVALUE:
                        configItem.MinValue = item.AsInt();
                        break;

                    case ModelCode.DISCRETE_NORMALVALUE:
                        configItem.DefaultValue = item.AsInt();
                        break;

                    case ModelCode.MEASUREMENT_ADDRESS:
                        configItem.Address = ushort.Parse(item.AsString());
                        break;

                    case ModelCode.MEASUREMENT_ISINPUT:
                        if (isAna)
                            configItem.RegistarType = (item.AsBool() == true) ? PointType.ANALOG_INPUT : PointType.ANALOG_OUTPUT;
                        else
                            configItem.RegistarType = (item.AsBool() == true) ? PointType.DIGITAL_INPUT : PointType.DIGITAL_OUTPUT;
                        break;

                    case ModelCode.ANALOG_CURRENTVALUE:
                        configItem.CurrentValue = item.AsFloat();
                        break;

                    case ModelCode.ANALOG_MAXVALUE:
                        configItem.MaxValue = item.AsFloat();
                        break;

                    case ModelCode.ANALOG_MINVALUE:
                        configItem.MinValue = item.AsFloat();
                        break;

                    case ModelCode.ANALOG_NORMALVALUE:
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
            return configItem;
        }
    }
}
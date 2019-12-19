using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.ServiceContracts;
using SCADA_Common;
using SCADA_Config_Data.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceModel;

namespace SCADA_Config_Data.Repository
{
    public class DataModelRepository
    {
        public ushort TcpPort { get; protected set; }
        public byte UnitAddress { get; protected set; }
        public string ServiceAddress { get; protected set; }
        //public FunctionExecutor functionExecutor { get; set; }
        private INetworkModelGDAContract gdaQueryProxy = null;
        public Dictionary<long, ConfigItem> Points;
        public Dictionary<long, Dictionary<ModelCode, Property>> NMS_Model_Props;
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
            NMS_Model_Props = new Dictionary<long, Dictionary<ModelCode, Property>>();
            TcpPort = ushort.Parse(ConfigurationManager.AppSettings["TcpPort"]);
            UnitAddress = byte.Parse(ConfigurationManager.AppSettings["UnitAddress"]);
            ServiceAddress = ConfigurationManager.AppSettings["ServiceAddress"];
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
                        if (isAna)
                            configItem.Address = 3000;
                        else
                            configItem.Address = 40;
                        //configItem.Address = ushort.Parse(item.AsString());
                        break;
                    case ModelCode.MEASUREMENT_ISINPUT:
                        prop.Add(item.Id, item);
                        if (isAna)
                            configItem.RegistarType = (item.AsBool() == true) ? PointType.ANALOG_INPUT : PointType.ANALOG_OUTPUT;
                        else
                            configItem.RegistarType = (item.AsBool() == true) ? PointType.DIGITAL_INPUT : PointType.DIGITAL_OUTPUT;
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
            }
            NMS_Model_Props.Add(gid, prop);
            return configItem;
        }
    }
}

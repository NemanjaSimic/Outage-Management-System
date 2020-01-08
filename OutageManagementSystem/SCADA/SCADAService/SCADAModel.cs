using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.ServiceProxies;
using Outage.SCADA.ModBus.Connection;
using Outage.SCADA.SCADA_Common;
using Outage.SCADA.SCADA_Config_Data.Configuration;
using Outage.SCADA.SCADA_Config_Data.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;

namespace Outage.SCADA.SCADAService
{
    public class SCADAModel
    {
        private ILogger logger = LoggerWrapper.Instance;

        //TODO: stanje "modela" -> npr string putanje ka dokumentu
        private DataModelRepository scadaModel = DataModelRepository.Instance;
        private Dictionary<DeltaOpType, List<long>> modelChanges;
        private Dictionary<long, ConfigItem> delta_Points;
        private ModelResourcesDesc modelRD = new ModelResourcesDesc();
        private ConfigWriter ConfigWriter;

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

        public SCADAModel()
        {
            delta_Points = new Dictionary<long, ConfigItem>();

        }

        public bool Notify(Dictionary<DeltaOpType, List<long>> modelChanges)
        {
            this.modelChanges = modelChanges;
            return true;
        }

        public bool Prepare()
        {
            try
            {
                foreach(long gid in scadaModel.Points.Keys)
                {
                    ConfigItem configItem = (ConfigItem)scadaModel.Points[gid].Clone();
                    delta_Points.Add(gid, configItem);
                }

                foreach (long gid in modelChanges[DeltaOpType.Delete])
                {
                    ModelCode type = modelRD.GetModelCodeFromId(gid);
                    if (type == ModelCode.ANALOG || type == ModelCode.DISCRETE)
                    {
                        if (delta_Points.ContainsKey(gid))
                        {
                            delta_Points.Remove(gid);
                        }
                    }
                }

                foreach (long gid in modelChanges[DeltaOpType.Insert])
                {
                    ModelCode type = modelRD.GetModelCodeFromId(gid);
                    if (type == ModelCode.ANALOG || type == ModelCode.DISCRETE)
                    {
                        ConfigItem configItem = CreateConfigItemForEntity(gid);
                        delta_Points.Add(gid, configItem);
                    }
                }

                foreach (long gid in modelChanges[DeltaOpType.Update])
                {
                    ModelCode type = modelRD.GetModelCodeFromId(gid);
                    if(type == ModelCode.ANALOG || type == ModelCode.DISCRETE)
                    {
                        ConfigItem configItem = CreateConfigItemForEntity(gid);
                        delta_Points[gid] = configItem;
                    }
                }

                ConfigWriter = new ConfigWriter(scadaModel.ConfigFileName, delta_Points.Values.ToList());
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public void Commit()
        {
            try
            {
                scadaModel.Points = delta_Points;
                
                if (File.Exists(scadaModel.PathToDeltaCfg))
                {
                    File.Delete(scadaModel.PathToDeltaCfg);
                }
                //Move validan u backup folder
                File.Move(scadaModel.PathMdbSimCfg, scadaModel.PathToDeltaCfg);


                
                if (File.Exists(scadaModel.PathMdbSimCfg))
                {
                    File.Delete(scadaModel.PathMdbSimCfg);
                }
                //Move u MdbSim folder
                File.Move(scadaModel.ConfigFileName, scadaModel.PathMdbSimCfg);

                string message = "There has been changes in configuration file.";
                Console.WriteLine(message);
                logger.LogInfo(message);

                ModbusSimulatorHandler.RestartSimulator();
                modelChanges.Clear();
            }
            catch (Exception e)
            {
                logger.LogError("Error on SCADA commit. ", e);
                throw e;
            }
        }

        public void Rollback()
        {
            if (File.Exists(scadaModel.PathMdbSimCfg))
            {
                File.Delete(scadaModel.PathMdbSimCfg);
            }
            //Move u MdbSim folder
            File.Move(scadaModel.PathToDeltaCfg, scadaModel.PathMdbSimCfg);
            ModbusSimulatorHandler.RestartSimulator();
            delta_Points.Clear();
            modelChanges.Clear();

        }

        public ConfigItem CreateConfigItemForEntity(long gid)
        {
            ModelCode type = modelRD.GetModelCodeFromId(gid);
            List<ModelCode> props;
            ResourceDescription rd;
            ConfigItem configItem = null;


            using(NetworkModelGDAProxy gdaProxy = GdaQueryProxy)
            {
                if(gdaProxy != null)
                {
                    if (type == ModelCode.ANALOG || type == ModelCode.DISCRETE)
                    {
                        props = modelRD.GetAllPropertyIds(type);
                        rd = gdaProxy.GetValues(gid, props);
                        configItem = scadaModel.ConfigurateConfigItem(rd.Properties, type);
                    }
                    else
                    {
                        string errMessage = $"ResourceDescription type is neither analog nor digital. Type: {type}.";
                        logger.LogWarn(errMessage);
                        configItem = null;
                        //throw new Exception(errMessage);
                    }
                }
                else
                {
                    string message = "NetworkModelGDAProxy is null.";
                    logger.LogWarn(message);
                    //TODO: retry logic?
                    throw new NullReferenceException(message);
                }
            }

            return configItem;
        }
      
        
    }
}
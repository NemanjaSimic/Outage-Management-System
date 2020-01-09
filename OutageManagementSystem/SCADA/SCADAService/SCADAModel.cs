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
using System.Threading;

namespace Outage.SCADA.SCADAService
{
    public class SCADAModel
    {
        private ILogger logger = LoggerWrapper.Instance;
        private ModelResourcesDesc modelRD;
        private ConfigWriter configWriter;

        private Dictionary<DeltaOpType, List<long>> modelChanges;
        private DataModelRepository dataModelRepository = DataModelRepository.Instance;
        private Dictionary<long, ConfigItem> incomingPoints;

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
                        logger.LogDebug($"SCADAModel: GdaQueryProxy getter, try number: {numberOfTries}.");
                        Thread.Sleep(500);
                    }
                }

                return gdaQueryProxy;
            }
        }
        #endregion

        public SCADAModel()
        {
            incomingPoints = new Dictionary<long, ConfigItem>();
            modelRD = new ModelResourcesDesc();
        }

        public bool Notify(Dictionary<DeltaOpType, List<long>> modelChanges)
        {
            this.modelChanges = modelChanges;
            return true;
        }

        public bool Prepare()
        {
            bool success = false;

            try
            {
                foreach(long gid in dataModelRepository.Points.Keys)
                {
                    ConfigItem point = (ConfigItem)dataModelRepository.Points[gid].Clone();
                    incomingPoints.Add(gid, point);
                }

                foreach (long gid in modelChanges[DeltaOpType.Insert])
                {
                    ModelCode type = modelRD.GetModelCodeFromId(gid);
                    if (type == ModelCode.ANALOG || type == ModelCode.DISCRETE)
                    {
                        ConfigItem configItem = CreateConfigItemForEntity(gid);
                        incomingPoints.Add(gid, configItem);
                    }
                }

                foreach (long gid in modelChanges[DeltaOpType.Update])
                {
                    ModelCode type = modelRD.GetModelCodeFromId(gid);
                    if(type == ModelCode.ANALOG || type == ModelCode.DISCRETE)
                    {
                        ConfigItem configItem = CreateConfigItemForEntity(gid);
                        incomingPoints[gid] = configItem;
                    }
                }

                foreach (long gid in modelChanges[DeltaOpType.Delete])
                {
                    ModelCode type = modelRD.GetModelCodeFromId(gid);
                    if (type == ModelCode.ANALOG || type == ModelCode.DISCRETE)
                    {
                        if (incomingPoints.ContainsKey(gid))
                        {
                            incomingPoints.Remove(gid);
                        }
                    }
                }


                //TODO: backup trenutne konfiguracije
                //generisanje nove configuracije, na lokaciji trenutne konfiguracije -> ConfigWriter(trenutna lokacija, incomingPoints.Values.ToList());

                throw new NotImplementedException("SCADA prepare (partialy finished)");

                //configWriter = new ConfigWriter(dataModelRepository.ConfigFileName, incomingPoints.Values.ToList());
                //configWriter.GenerateConfigFile();

                success = true;
            }
            catch (Exception e)
            {
                logger.LogError($"Exception catched in Prepare method on SCADAModel.", e);
                success = false;
            }

            return success;
        }

        public void Commit()
        {
            //TOOD:
            //brisanje backup konfiguracije
            //pokretanje sim sa novom -> mozda ne iz ovog commita nego iz onog wcf-ovskog koji se gadja

            throw new NotImplementedException("SCADA Commit");

            //dataModelRepository.Points = incomingPoints;

            //if (File.Exists(dataModelRepository.IncomingConfigPath))
            //{
            //    File.Delete(dataModelRepository.IncomingConfigPath);
            //}

            //if (File.Exists(dataModelRepository.CurrentConfigPath))
            //{
            //    //Move validan u backup folder
            //    File.Move(dataModelRepository.CurrentConfigPath, dataModelRepository.IncomingConfigPath);
            //}

            //if (File.Exists(dataModelRepository.CurrentConfigPath))
            //{
            //    File.Delete(dataModelRepository.CurrentConfigPath);
            //}

            //if (File.Exists(dataModelRepository.CurrentConfigPath))
            //{
            //    //Move u MdbSim folder
            //    File.Move(dataModelRepository.ConfigFileName, dataModelRepository.CurrentConfigPath);
            //}

            //string message = "There has been changes in configuration file.";
            //Console.WriteLine(message);
            //logger.LogInfo(message);

            //modelChanges.Clear();

            //ModbusSimulatorHandler.RestartSimulator();
        }

        public void Rollback()
        {
            //TODO:
            //brisanje trenutne configuracje
            //vracanje stare iz backupfoldera

            throw new NotImplementedException("SCADA Rollback");


            //if (File.Exists(dataModelRepository.CurrentConfigPath))
            //{
            //    File.Delete(dataModelRepository.CurrentConfigPath);
            //}

            ////Move u MdbSim folder
            //File.Move(dataModelRepository.IncomingConfigPath, dataModelRepository.CurrentConfigPath);

            //incomingPoints.Clear();
            //modelChanges.Clear();

            //ModbusSimulatorHandler.RestartSimulator();
        }

        private ConfigItem CreateConfigItemForEntity(long gid)
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
                        configItem = dataModelRepository.ConfigurateConfigItem(rd.Properties, type);
                    }
                    else
                    {
                        string errMessage = $"ResourceDescription type is neither analog nor digital. Type: {type}.";
                        logger.LogWarn(errMessage);
                        configItem = null;
                    }
                }
                else
                {
                    string message = "From method CreateConfigItemForEntity(): NetworkModelGDAProxy is null.";
                    logger.LogWarn(message);
                    throw new NullReferenceException(message);
                }
            }

            return configItem;
        }
    }
}
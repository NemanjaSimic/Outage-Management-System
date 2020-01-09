using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.ServiceProxies;
using Outage.SCADA.ModBus.Connection;
using Outage.SCADA.SCADACommon;
using Outage.SCADA.SCADAConfigData;
using Outage.SCADA.SCADAConfigData.Configuration;
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
        private ModelResourcesDesc modelResourceDesc;
        //private ConfigWriter configWriter;

        private Dictionary<DeltaOpType, List<long>> modelChanges;
        private SCADAConfigData.Configuration.SCADAConfigData dataModelRepository = SCADAConfigData.Configuration.SCADAConfigData.Instance;

        private Dictionary<long, ModbusPoint> currentScadaModel;
        protected Dictionary<long, ModbusPoint> CurrentScadaModel
        {
            get { return currentScadaModel ?? (currentScadaModel = new Dictionary<long, ModbusPoint>()); }
        }

        private Dictionary<long, ModbusPoint> incomingScadaModel;
        protected Dictionary<long, ModbusPoint> IncomingScadaModel
        {
            get { return incomingScadaModel ?? (incomingScadaModel = new Dictionary<long, ModbusPoint>()); }
        }

        [Obsolete]
        public Dictionary<long, ResourceDescription> NetworkModel { get; protected set; }
        [Obsolete]
        public Dictionary<long, Dictionary<ModelCode, Property>> NetworkModelProps { get; protected set; }


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
            currentScadaModel = new Dictionary<long, ModbusPoint>();
            incomingScadaModel = new Dictionary<long, ModbusPoint>();
            modelResourceDesc = new ModelResourcesDesc();
        }

        public bool Notify(Dictionary<DeltaOpType, List<long>> modelChanges)
        {
            this.modelChanges = modelChanges;
            return true;
        }

        public bool Prepare()
        {
            bool success;

            try
            {
                foreach(long gid in CurrentScadaModel.Keys)
                {
                    ModbusPoint point = (ModbusPoint)CurrentScadaModel[gid].Clone();
                    IncomingScadaModel.Add(gid, point);
                }

                foreach (long gid in modelChanges[DeltaOpType.Insert])
                {
                    ModelCode type = modelResourceDesc.GetModelCodeFromId(gid);
                    if (type == ModelCode.ANALOG || type == ModelCode.DISCRETE)
                    {
                        ModbusPoint configItem = CreateConfigItemForEntity(gid);
                        IncomingScadaModel.Add(gid, configItem);
                    }
                }

                foreach (long gid in modelChanges[DeltaOpType.Update])
                {
                    ModelCode type = modelResourceDesc.GetModelCodeFromId(gid);
                    if(type == ModelCode.ANALOG || type == ModelCode.DISCRETE)
                    {
                        ModbusPoint configItem = CreateConfigItemForEntity(gid);
                        IncomingScadaModel[gid] = configItem;
                    }
                }

                foreach (long gid in modelChanges[DeltaOpType.Delete])
                {
                    ModelCode type = modelResourceDesc.GetModelCodeFromId(gid);
                    if (type == ModelCode.ANALOG || type == ModelCode.DISCRETE)
                    {
                        if (IncomingScadaModel.ContainsKey(gid))
                        {
                            IncomingScadaModel.Remove(gid);
                        }
                    }
                }

                //configWriter = new ConfigWriter(dataModelRepository.ConfigFileName, IncomingScadaModel.Values.ToList());
                //configWriter.GenerateConfigFile();

                //if (File.Exists(dataModelRepository.BackupConfigPath))
                //{
                //    File.Delete(dataModelRepository.BackupConfigPath);
                //}

                //if (File.Exists(dataModelRepository.CurrentConfigPath))
                //{
                //    //Move current config to backup folder
                //    File.Move(dataModelRepository.CurrentConfigPath, dataModelRepository.BackupConfigPath);
                //}

                //if (File.Exists(dataModelRepository.CurrentConfigPath))
                //{
                //    File.Delete(dataModelRepository.CurrentConfigPath);
                //}

                //if (File.Exists(dataModelRepository.ConfigFileName))
                //{
                //    //Move u MdbSim folder
                //    File.Move(dataModelRepository.ConfigFileName, dataModelRepository.CurrentConfigPath);
                //}

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
            //dataModelRepository.Points = IncomingScadaModel;

            currentScadaModel = IncomingScadaModel;
            incomingScadaModel = null;
            modelChanges.Clear();

            //if (File.Exists(dataModelRepository.BackupConfigPath))
            //{
            //    File.Delete(dataModelRepository.BackupConfigPath);
            //}

            string message = $"Incoming config file is confirmed.";
            Console.WriteLine(message);
            logger.LogInfo(message);
        }

        public void Rollback()
        {
            incomingScadaModel = null;
            modelChanges.Clear();

            //if (File.Exists(dataModelRepository.CurrentConfigPath))
            //{
            //    File.Delete(dataModelRepository.CurrentConfigPath);
            //}

            //if (File.Exists(dataModelRepository.BackupConfigPath))
            //{
            //    File.Move(dataModelRepository.BackupConfigPath, dataModelRepository.CurrentConfigPath);
            //}
            //else
            //{
            //    if (File.Exists(dataModelRepository.ConfigFileName))
            //    {
            //        File.Move(dataModelRepository.ConfigFileName, dataModelRepository.CurrentConfigPath);
            //    }
            //    else
            //    {
            //        configWriter = new ConfigWriter(dataModelRepository.ConfigFileName, dataModelRepository.Points.Values.ToList());
            //        configWriter.GenerateConfigFile();
            //        File.Move(dataModelRepository.ConfigFileName, dataModelRepository.CurrentConfigPath);
            //    }
            //}
        }

        #region ImportScadaModel
        private bool ImportModel()
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
            List<ModelCode> props = modelResourceDesc.GetAllPropertyIds(ModelCode.ANALOG);

            try
            {
                using (NetworkModelGDAProxy gdaProxy = GdaQueryProxy)
                {
                    if (gdaProxy != null)
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
                                    //NetworkModel.Add(rds[i].Id, rds[i]);
                                    ModbusPoint point = new ModbusPoint(rds[i].Properties, ModelCode.ANALOG);
                                    CurrentScadaModel.Add(rds[i].Id, point);
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
            int numberOfResources = 1000;
            List<ModelCode> props = modelResourceDesc.GetAllPropertyIds(ModelCode.DISCRETE);

            try
            {
                using (NetworkModelGDAProxy gdaProxy = GdaQueryProxy)
                {
                    if (gdaProxy != null)
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
                                    //NetworkModel.Add(rds[i].Id, rds[i]);
                                    ModbusPoint point = new ModbusPoint(rds[i].Properties, ModelCode.DISCRETE);
                                    CurrentScadaModel.Add(rds[i].Id, point);
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
        #endregion

        private ModbusPoint CreateConfigItemForEntity(long gid)
        {
            ModelCode type = modelResourceDesc.GetModelCodeFromId(gid);
            List<ModelCode> props;
            ResourceDescription rd;
            ModbusPoint configItem;

            using(NetworkModelGDAProxy gdaProxy = GdaQueryProxy)
            {
                if(gdaProxy != null)
                {
                    if (type == ModelCode.ANALOG || type == ModelCode.DISCRETE)
                    {
                        props = modelResourceDesc.GetAllPropertyIds(type);
                        rd = gdaProxy.GetValues(gid, props);
                        configItem = new ModbusPoint(rd.Properties, type);
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
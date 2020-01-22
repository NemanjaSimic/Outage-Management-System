using EasyModbus;
using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.ServiceProxies;
using Outage.SCADA.SCADACommon;
using Outage.SCADA.SCADAData.Configuration;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Outage.SCADA.SCADAData.Repository
{
    public class SCADAModel
    {
        private ILogger logger;

        protected ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        private ModelResourcesDesc modelResourceDesc;

        private bool isSCADAModelImported;
        private Dictionary<DeltaOpType, List<long>> modelChanges;
        private Dictionary<long, ISCADAModelPointItem> incomingScadaModel;
        private Dictionary<PointType, Dictionary<ushort, long>> incomingAddressToGidMap;
        private Dictionary<long, ISCADAModelPointItem> currentScadaModel;
        private Dictionary<PointType, Dictionary<ushort, long>> currentAddressToGidMap;
        


        #region Properties

        protected Dictionary<DeltaOpType, List<long>> ModelChanges
        {
            get { return modelChanges ?? (modelChanges = new Dictionary<DeltaOpType, List<long>>()); }
        }

        protected Dictionary<long, ISCADAModelPointItem> IncomingScadaModel
        {
            get { return incomingScadaModel ?? (incomingScadaModel = new Dictionary<long, ISCADAModelPointItem>()); }
        }

        protected Dictionary<PointType, Dictionary<ushort, long>> IncomingAddressToGidMap
        {
            get
            {
                return incomingAddressToGidMap ?? (incomingAddressToGidMap = new Dictionary<PointType, Dictionary<ushort, long>>() 
                {  
                    { PointType.ANALOG_INPUT,   new Dictionary<ushort, long>()  },
                    { PointType.ANALOG_OUTPUT,  new Dictionary<ushort, long>()  },
                    { PointType.DIGITAL_INPUT,  new Dictionary<ushort, long>()  },
                    { PointType.DIGITAL_OUTPUT, new Dictionary<ushort, long>()  },
                    { PointType.HR_LONG,        new Dictionary<ushort, long>()  },
                }); 
            }
        }


        public bool IsSCADAModelImported
        {
            get { return isSCADAModelImported;  }
        }

        public Dictionary<long, ISCADAModelPointItem> CurrentScadaModel
        {
            get { return currentScadaModel ?? (currentScadaModel = new Dictionary<long, ISCADAModelPointItem>()); }
        }

        public Dictionary<PointType, Dictionary<ushort, long>> CurrentAddressToGidMap
        {
            get 
            { 
                return currentAddressToGidMap ?? (currentAddressToGidMap = new Dictionary<PointType, Dictionary<ushort, long>>() 
                {
                    { PointType.ANALOG_INPUT,   new Dictionary<ushort, long>()  },
                    { PointType.ANALOG_OUTPUT,  new Dictionary<ushort, long>()  },
                    { PointType.DIGITAL_INPUT,  new Dictionary<ushort, long>()  },
                    { PointType.DIGITAL_OUTPUT, new Dictionary<ushort, long>()  },
                    { PointType.HR_LONG,        new Dictionary<ushort, long>()  },
                });
            }
        }

        public event ModelUpdateDelegate SignalIncomingModelConfirmation;
        
        #endregion Properties

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
                        Logger.LogError(message, ex);
                        gdaQueryProxy = null;
                    }
                    finally
                    {
                        numberOfTries++;
                        Logger.LogDebug($"SCADAModel: GdaQueryProxy getter, try number: {numberOfTries}.");
                        Thread.Sleep(500);
                    }
                }

                return gdaQueryProxy;
            }
        }

        #endregion Proxies

        public SCADAModel()
        {
            currentScadaModel = new Dictionary<long, ISCADAModelPointItem>();
            incomingScadaModel = new Dictionary<long, ISCADAModelPointItem>();
            modelResourceDesc = new ModelResourcesDesc();
        }

        #region IModelUpdateNotificationContract

        public bool Notify(Dictionary<DeltaOpType, List<long>> modelChanges)
        {
            this.modelChanges = modelChanges;
            return true;
        }

        #endregion IModelUpdateNotificationContract

        #region ITransactionActorContract

        public bool Prepare()
        {
            bool success;
            try
            {
                incomingScadaModel = new Dictionary<long, ISCADAModelPointItem>(CurrentScadaModel.Count);
                
                foreach (long gid in CurrentScadaModel.Keys)
                {
                    ModelCode type = modelResourceDesc.GetModelCodeFromId(gid);
                    ISCADAModelPointItem pointItem = CurrentScadaModel[gid].Clone();

                    IncomingScadaModel.Add(gid, pointItem);
                    
                    if(!IncomingAddressToGidMap[pointItem.RegisterType].ContainsKey(pointItem.Address))
                    {
                        IncomingAddressToGidMap[pointItem.RegisterType].Add(pointItem.Address, gid);
                    }
                }

                foreach (long gid in modelChanges[DeltaOpType.Insert])
                {
                    ModelCode type = modelResourceDesc.GetModelCodeFromId(gid);
                    if (type == ModelCode.ANALOG || type == ModelCode.DISCRETE)
                    {
                        ISCADAModelPointItem pointItem = CreateConfigItemForEntity(gid);
                        
                        if (IncomingScadaModel.ContainsKey(gid) || IncomingAddressToGidMap[pointItem.RegisterType].ContainsKey(pointItem.Address))
                        {
                            string message = $"Model update data in fault state. Inserting gid: {gid} or measurement address: {pointItem.Address}, that already exists in SCADA model.";
                            Logger.LogError(message);
                            throw new ArgumentException(message);
                        }

                        IncomingScadaModel.Add(gid, pointItem);
                        IncomingAddressToGidMap[pointItem.RegisterType].Add(pointItem.Address, gid);
                    }
                }

                foreach (long gid in modelChanges[DeltaOpType.Update])
                {
                    ModelCode type = modelResourceDesc.GetModelCodeFromId(gid);
                    if (type == ModelCode.ANALOG || type == ModelCode.DISCRETE)
                    {
                        ISCADAModelPointItem pointItem = CreateConfigItemForEntity(gid);

                        if (!IncomingScadaModel.ContainsKey(gid))
                        {
                            string message = $"Model update data in fault state. Updating entity with gid: {gid} or measurement address: {pointItem.Address}, that does not exists in SCADA model.";
                            Logger.LogError(message);
                            throw new ArgumentException(message);
                        }

                        ISCADAModelPointItem oldPointItem = IncomingScadaModel[gid];
                        IncomingScadaModel[gid] = pointItem;
                        IncomingAddressToGidMap[pointItem.RegisterType].Remove(oldPointItem.Address);
                        IncomingAddressToGidMap[pointItem.RegisterType][pointItem.Address] = gid;
                    }
                }

                foreach (long gid in modelChanges[DeltaOpType.Delete])
                {
                    ModelCode type = modelResourceDesc.GetModelCodeFromId(gid);
                    if (type == ModelCode.ANALOG || type == ModelCode.DISCRETE)
                    {
                        if (!IncomingScadaModel.ContainsKey(gid))
                        {
                            string message = $"Model update data in fault state. Deleting entity with gid: {gid}, that does not exists in SCADA model.";
                            Logger.LogError(message);
                            throw new ArgumentException(message);
                        }

                        ushort address = IncomingScadaModel[gid].Address;
                        IncomingAddressToGidMap[IncomingScadaModel[gid].RegisterType].Remove(address);
                        IncomingScadaModel.Remove(gid);
                    }
                }

                success = true;
            }
            catch (Exception e)
            {
                Logger.LogError($"Exception caught in Prepare method on SCADAModel.", e);
                success = false;
            }

            return success;
        }

        public void Commit()
        {
            currentScadaModel = IncomingScadaModel;
            incomingScadaModel = null;

            currentAddressToGidMap = IncomingAddressToGidMap;
            incomingAddressToGidMap = null;

            modelChanges.Clear();

            string message = $"Incoming SCADA model is confirmed.";
            Console.WriteLine(message);
            Logger.LogInfo(message);
            
            SignalIncomingModelConfirmation.Invoke(new List<long>(CurrentScadaModel.Keys));
        }

        public void Rollback()
        {
            
            incomingScadaModel = null;
            incomingAddressToGidMap = null;
            modelChanges.Clear();

            string message = $"Incoming SCADA model is rejected.";
            Console.WriteLine(message);
            Logger.LogInfo(message);
        }

        #endregion ITransactionActorContract

        #region ImportScadaModel

        public bool ImportModel()
        {
            string message = "Importing analog measurements started...";
            Logger.LogInfo(message);
            Console.WriteLine(message);
            bool analogImportSuccess = ImportAnalog();

            message = $"Importing analog measurements finished. ['success' value: {analogImportSuccess}]";
            Logger.LogInfo(message);
            Console.WriteLine(message);

            message = "Importing discrete measurements started...";
            Logger.LogInfo(message);
            Console.WriteLine(message);
            bool discreteImportSuccess = ImportDiscrete();

            message = $"Importing discrete measurements finished. ['success' value: {discreteImportSuccess}]";
            Logger.LogInfo(message);
            Console.WriteLine(message);

            isSCADAModelImported = analogImportSuccess && discreteImportSuccess;
            
            if (isSCADAModelImported && SignalIncomingModelConfirmation != null)
            {
                SignalIncomingModelConfirmation.Invoke(new List<long>(CurrentScadaModel.Keys));
            }

            return isSCADAModelImported;
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
                                    long gid = rds[i].Id;
                                    ModelCode type = modelResourceDesc.GetModelCodeFromId(gid);
                                    ISCADAModelPointItem pointItem = new AnalogSCADAModelPointItem(rds[i].Properties, ModelCode.ANALOG);
                                    CurrentScadaModel.Add(rds[i].Id, pointItem);
                                    CurrentAddressToGidMap[pointItem.RegisterType].Add(pointItem.Address, rds[i].Id);
                                    
                                    Logger.LogDebug($"ANALOG measurement added to SCADA model [Gid: {gid}, Address: {pointItem.Address}]");
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
                        Logger.LogWarn(errMsg);
                    }
                }
            }
            catch (Exception ex)
            {
                success = false;
                string errorMessage = $"ImportAnalog failed with error: {ex.Message}";
                Console.WriteLine(errorMessage);
                Logger.LogError(errorMessage, ex);
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
                                    long gid = rds[i].Id;
                                    ModelCode type = modelResourceDesc.GetModelCodeFromId(gid);
                                    ISCADAModelPointItem pointItem = new DiscreteSCADAModelPointItem(rds[i].Properties, ModelCode.DISCRETE);
                                    CurrentScadaModel.Add(gid, pointItem);
                                    CurrentAddressToGidMap[pointItem.RegisterType].Add(pointItem.Address, gid);
                                    Logger.LogDebug($"DISCRETE measurement added to SCADA model [Gid: {gid}, Address: {pointItem.Address}]");
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
                        Logger.LogWarn(errMsg);
                    }
                }
            }
            catch (Exception ex)
            {
                success = false;
                string errorMessage = $"ImportDiscrete failed with error: {ex.Message}";
                Console.WriteLine(errorMessage);
                Logger.LogError(errorMessage, ex);
            }

            return success;
        }

        #endregion ImportScadaModel

        private ISCADAModelPointItem CreateConfigItemForEntity(long gid)
        {
            ModelCode type = modelResourceDesc.GetModelCodeFromId(gid);
            List<ModelCode> props;
            ResourceDescription rd;
            ISCADAModelPointItem pointItem;

            using (NetworkModelGDAProxy gdaProxy = GdaQueryProxy)
            {
                if (gdaProxy != null)
                {
                    if (type == ModelCode.ANALOG)
                    {
                        props = modelResourceDesc.GetAllPropertyIds(type);
                        rd = gdaProxy.GetValues(gid, props);
                        pointItem = new AnalogSCADAModelPointItem(rd.Properties, type);
                    }
                    else if (type == ModelCode.DISCRETE)
                    {
                        props = modelResourceDesc.GetAllPropertyIds(type);
                        rd = gdaProxy.GetValues(gid, props);
                        pointItem = new DiscreteSCADAModelPointItem(rd.Properties, type);
                    }
                    else
                    {
                        string errMessage = $"ResourceDescription type is neither analog nor digital. Type: {type}.";
                        Logger.LogWarn(errMessage);
                        pointItem = null;
                    }
                }
                else
                {
                    string message = "From method CreateConfigItemForEntity(): NetworkModelGDAProxy is null.";
                    Logger.LogWarn(message);
                    throw new NullReferenceException(message);
                }
            }

            return pointItem;
        }
        
    }
}
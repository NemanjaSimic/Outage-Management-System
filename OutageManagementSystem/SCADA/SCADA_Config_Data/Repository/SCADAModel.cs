using EasyModbus;
using Outage.Common;
using Outage.Common.GDA;
using Outage.Common.ServiceContracts.GDA;
using Outage.Common.ServiceProxies;
using Outage.SCADA.SCADACommon;
using Outage.SCADA.SCADAData.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;

namespace Outage.SCADA.SCADAData.Repository
{
    public sealed class SCADAModel
    {
        private ILogger logger;

        private ILogger Logger
        {
            get { return logger ?? (logger = LoggerWrapper.Instance); }
        }

        private EnumDescs enumDescs;
        private ModelResourcesDesc modelResourceDesc;
        private ProxyFactory proxyFactory;

        private bool isSCADAModelImported;
        private Dictionary<DeltaOpType, List<long>> modelChanges;
        private Dictionary<long, ISCADAModelPointItem> incomingScadaModel;
        private Dictionary<PointType, Dictionary<ushort, long>> incomingAddressToGidMap;
        private Dictionary<long, ISCADAModelPointItem> currentScadaModel;
        private Dictionary<PointType, Dictionary<ushort, long>> currentAddressToGidMap;
        private Dictionary<long, CommandValue> commandedValuesCache;

        #region Properties

        private Dictionary<DeltaOpType, List<long>> ModelChanges
        {
            get { return modelChanges ?? (modelChanges = new Dictionary<DeltaOpType, List<long>>()); }
        }

        private Dictionary<long, ISCADAModelPointItem> IncomingScadaModel
        {
            get { return incomingScadaModel ?? (incomingScadaModel = new Dictionary<long, ISCADAModelPointItem>()); }
        }

        private Dictionary<PointType, Dictionary<ushort, long>> IncomingAddressToGidMap
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
            get { return isSCADAModelImported; }
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

        public Dictionary<long, CommandValue> CommandedValuesCache
        {
            get { return commandedValuesCache ?? (commandedValuesCache = new Dictionary<long, CommandValue>()); }
        }

        public event ModelUpdateDelegate SignalIncomingModelConfirmation;

        #endregion Properties

        public SCADAModel(ModelResourcesDesc modelResourceDesc, EnumDescs enumDescs)
        {
            this.modelResourceDesc = modelResourceDesc;
            this.enumDescs = enumDescs;
            this.proxyFactory = new ProxyFactory();

            currentScadaModel = new Dictionary<long, ISCADAModelPointItem>();
            incomingScadaModel = new Dictionary<long, ISCADAModelPointItem>();
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
                //INIT INCOMING SCADA MODEL with current model values
                //can not go with just 'incomingScadaModel = new Dictionary<long, ISCADAModelPointItem>(CurrentScadaModel)' because IncomingAddressToGidMap must also be initialized
                incomingScadaModel = new Dictionary<long, ISCADAModelPointItem>(CurrentScadaModel.Count);

                foreach (long gid in CurrentScadaModel.Keys)
                {
                    ModelCode type = modelResourceDesc.GetModelCodeFromId(gid);
                    ISCADAModelPointItem pointItem = CurrentScadaModel[gid].Clone();

                    IncomingScadaModel.Add(gid, pointItem);

                    if (!IncomingAddressToGidMap[pointItem.RegisterType].ContainsKey(pointItem.Address))
                    {
                        IncomingAddressToGidMap[pointItem.RegisterType].Add(pointItem.Address, gid);
                    }
                }

                //IMPORT ALL measurements from NMS and create PointItems for them
                CreatePointItemsFromNetworkModelMeasurements(out Dictionary<long, ISCADAModelPointItem> incomingPointItems);

                //ORDER IS IMPORTANT due to IncomingAddressToGidMap validity: DELETE => UPDATE => INSERT

                foreach (long gid in modelChanges[DeltaOpType.Delete])
                {
                    ModelCode type = modelResourceDesc.GetModelCodeFromId(gid);
                    if (type == ModelCode.ANALOG || type == ModelCode.DISCRETE)
                    {
                        if (!IncomingScadaModel.ContainsKey(gid))
                        {
                            success = false;
                            string message = $"Model update data in fault state. Deleting entity with gid: {gid}, that does not exists in SCADA model.";
                            Logger.LogError(message);
                            throw new ArgumentException(message);
                        }

                        ISCADAModelPointItem oldPointItem = IncomingScadaModel[gid];
                        IncomingScadaModel.Remove(gid);

                        IncomingAddressToGidMap[oldPointItem.RegisterType].Remove(oldPointItem.Address);
                    }
                }

                foreach (long gid in modelChanges[DeltaOpType.Update])
                {
                    ModelCode type = modelResourceDesc.GetModelCodeFromId(gid);
                    if (type == ModelCode.ANALOG || type == ModelCode.DISCRETE)
                    {

                        if (!IncomingScadaModel.ContainsKey(gid))
                        {
                            success = false;
                            string message = $"Model update data in fault state. Updating entity with gid: 0x{gid:X16}, that does not exists in SCADA model.";
                            Logger.LogError(message);
                            throw new ArgumentException(message);
                        }

                        ISCADAModelPointItem incomingPointItem = incomingPointItems[gid];
                        ISCADAModelPointItem oldPointItem = IncomingScadaModel[gid];

                        if (!IncomingAddressToGidMap[oldPointItem.RegisterType].ContainsKey(oldPointItem.Address))
                        {
                            success = false;
                            string message = $"Model update data in fault state. Updating point with address: {oldPointItem.Address}, that does not exists in SCADA model.";
                            Logger.LogError(message);
                            throw new ArgumentException(message);
                        }

                        if (oldPointItem.Address != incomingPointItem.Address && IncomingAddressToGidMap[incomingPointItem.RegisterType].ContainsKey(incomingPointItem.Address))
                        {
                            success = false;
                            string message = $"Model update data in fault state. Trying to add point with address: {incomingPointItem.Address}, that already exists in SCADA model.";
                            Logger.LogError(message);
                            throw new ArgumentException(message);
                        }

                        IncomingScadaModel[gid] = incomingPointItem;

                        if (oldPointItem.Address != incomingPointItem.Address)
                        {
                            IncomingAddressToGidMap[oldPointItem.RegisterType].Remove(oldPointItem.Address);
                            IncomingAddressToGidMap[incomingPointItem.RegisterType].Add(incomingPointItem.Address, gid);
                        }

                    }
                }

                foreach (long gid in modelChanges[DeltaOpType.Insert])
                {
                    ModelCode type = modelResourceDesc.GetModelCodeFromId(gid);
                    if (type == ModelCode.ANALOG || type == ModelCode.DISCRETE)
                    {
                        if (IncomingScadaModel.ContainsKey(gid))
                        {
                            success = false;
                            string message = $"Model update data in fault state. Inserting gid: {gid}, that already exists in SCADA model.";
                            Logger.LogError(message);
                            throw new ArgumentException(message);
                        }

                        ISCADAModelPointItem incomingPointItem = incomingPointItems[gid];

                        if (IncomingAddressToGidMap[incomingPointItem.RegisterType].ContainsKey(incomingPointItem.Address))
                        {
                            success = false;
                            string message = $"Model update data in fault state. Trying to add point with address: {incomingPointItem.Address}, that already exists in SCADA model.";
                            Logger.LogError(message);
                            throw new ArgumentException(message);
                        }

                        IncomingScadaModel.Add(gid, incomingPointItem);
                        IncomingAddressToGidMap[incomingPointItem.RegisterType].Add(incomingPointItem.Address, gid);
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
            CommandedValuesCache.Clear();

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

            using (NetworkModelGDAProxy gdaProxy = proxyFactory.CreateProxy<NetworkModelGDAProxy, INetworkModelGDAContract>(EndpointNames.NetworkModelGDAEndpoint))
            {
                if (gdaProxy == null)
                {
                    success = false;
                    string errMsg = "From ImportAnalog() method: NetworkModelGDAProxy is null.";
                    Logger.LogWarn(errMsg);
                }

                try
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
                                ISCADAModelPointItem pointItem = new AnalogSCADAModelPointItem(rds[i].Properties, ModelCode.ANALOG, enumDescs);
                                CurrentScadaModel.Add(rds[i].Id, pointItem);
                                CurrentAddressToGidMap[pointItem.RegisterType].Add(pointItem.Address, rds[i].Id);

                                Logger.LogDebug($"ANALOG measurement added to SCADA model [Gid: {gid}, Address: {pointItem.Address}]");
                            }
                        }
                        resourcesLeft = gdaProxy.IteratorResourcesLeft(iteratorId);
                    }

                    success = true;
                }
                catch (Exception ex)
                {

                    success = false;
                    string errorMessage = $"ImportAnalog failed with error: {ex.Message}";
                    Console.WriteLine(errorMessage);
                    Logger.LogError(errorMessage, ex);
                }
            }

            return success;
        }

        private bool ImportDiscrete()
        {
            bool success;
            int numberOfResources = 1000;
            List<ModelCode> props = modelResourceDesc.GetAllPropertyIds(ModelCode.DISCRETE);

            using (NetworkModelGDAProxy gdaProxy = proxyFactory.CreateProxy<NetworkModelGDAProxy, INetworkModelGDAContract>(EndpointNames.NetworkModelGDAEndpoint))
            {
                if (gdaProxy == null)
                {
                    success = false;
                    string errMsg = "From ImportDiscrete() method: NetworkModelGDAProxy is null.";
                    Logger.LogWarn(errMsg);
                }

                try
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
                                ISCADAModelPointItem pointItem = new DiscreteSCADAModelPointItem(rds[i].Properties, ModelCode.DISCRETE, enumDescs);
                                CurrentScadaModel.Add(gid, pointItem);
                                CurrentAddressToGidMap[pointItem.RegisterType].Add(pointItem.Address, gid);
                                Logger.LogDebug($"DISCRETE measurement added to SCADA model [Gid: {gid}, Address: {pointItem.Address}]");
                            }
                        }
                        resourcesLeft = gdaProxy.IteratorResourcesLeft(iteratorId);
                    }

                    success = true;
                }
                catch (Exception ex)
                {

                    success = false;
                    string errorMessage = $"ImportDiscrete failed with error: {ex.Message}";
                    Console.WriteLine(errorMessage);
                    Logger.LogError(errorMessage, ex);
                } 
            }

            return success;
        }

        #endregion ImportScadaModel

        private void CreatePointItemsFromNetworkModelMeasurements(out Dictionary<long, ISCADAModelPointItem> pointItems)
        {
            pointItems = new Dictionary<long, ISCADAModelPointItem>();

            using (NetworkModelGDAProxy gdaProxy = proxyFactory.CreateProxy<NetworkModelGDAProxy, INetworkModelGDAContract>(EndpointNames.NetworkModelGDAEndpoint))
            {
                if (gdaProxy == null)
                {
                    string message = "From method CreatePointItemsFromNetworkModelMeasurements(): NetworkModelGDAProxy is null.";
                    Logger.LogWarn(message);
                    throw new NullReferenceException(message);
                }

                //ModelCode type;

                int iteratorId;
                int resourcesLeft;
                int numberOfResources = 10000;

                List<ModelCode> props;

                //TOOD: change service contract IModelUpdateNotificationContract to receive types of all changed elements from NMS 
                HashSet<ModelCode> changedTypes = new HashSet<ModelCode>();
                foreach (List<long> gids in ModelChanges.Values)
                {
                    foreach (long gid in gids)
                    {
                        ModelCode type = modelResourceDesc.GetModelCodeFromId(gid);

                        if (!changedTypes.Contains(type))
                        {
                            changedTypes.Add(type);
                        }
                    }
                }

                foreach (ModelCode type in changedTypes)
                {
                    if (type != ModelCode.ANALOG && type != ModelCode.DISCRETE)
                    {
                        continue;
                    }

                    props = modelResourceDesc.GetAllPropertyIds(type);

                    try
                    {
                        iteratorId = gdaProxy.GetExtentValues(type, props);
                        resourcesLeft = gdaProxy.IteratorResourcesLeft(iteratorId);

                        while (resourcesLeft > 0)
                        {
                            List<ResourceDescription> resources = gdaProxy.IteratorNext(numberOfResources, iteratorId);

                            foreach (ResourceDescription rd in resources)
                            {
                                if (pointItems.ContainsKey(rd.Id))
                                {
                                    string message = $"Trying to create point item for resource that already exists in model. Gid: 0x{rd.Id:X16}";
                                    Logger.LogError(message);
                                    throw new ArgumentException(message);
                                }

                                ISCADAModelPointItem point;

                                //change service contract IModelUpdateNotificationContract => change List<long> to Hashset<long> 
                                if (ModelChanges[DeltaOpType.Update].Contains(rd.Id) || ModelChanges[DeltaOpType.Insert].Contains(rd.Id))
                                {
                                    point = CreatePointItemFromResource(rd);
                                    pointItems.Add(rd.Id, point);
                                }
                            }

                            resourcesLeft = gdaProxy.IteratorResourcesLeft(iteratorId);
                        }

                        gdaProxy.IteratorClose(iteratorId);
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = $"CreatePointItemsFromNetworkModelMeasurements failed with error: {ex.Message}";
                        Console.WriteLine(errorMessage);
                        Logger.LogError(errorMessage, ex);
                    }
                }
            }
        }

        private ISCADAModelPointItem CreatePointItemFromResource(ResourceDescription resource)
        {
            long gid = resource.Id;
            ModelCode type = modelResourceDesc.GetModelCodeFromId(gid);

            ISCADAModelPointItem pointItem;

            if (type == ModelCode.ANALOG)
            {
                pointItem = new AnalogSCADAModelPointItem(resource.Properties, type, enumDescs);
            }
            else if (type == ModelCode.DISCRETE)
            {
                pointItem = new DiscreteSCADAModelPointItem(resource.Properties, type, enumDescs);
            }
            else
            {
                string errMessage = $"ResourceDescription type is neither analog nor digital. Type: {type}.";
                Logger.LogWarn(errMessage);
                pointItem = null;
            }

            return pointItem;
        }

    }
}
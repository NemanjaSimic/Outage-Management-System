using Common.SCADA;
using Microsoft.ServiceFabric.Data;
using OMS.Cloud.SCADA.Data.Repository;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.Cloud.WcfServiceFabricClients.NMS;
using OMS.Common.DistributedTransactionContracts;
using OMS.Common.NmsContracts.GDA;
using OMS.Common.SCADA;
using Outage.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.Cloud.SCADA.ModelProviderService
{
    internal sealed class ScadaModel : IModelUpdateNotificationContract, ITransactionActorContract
    {
        private ILogger logger;
        private ILogger Logger { get { return logger ?? (logger = LoggerWrapper.Instance); } }

        private readonly EnumDescs enumDescs;
        private readonly ModelResourcesDesc modelResourceDesc;
        private readonly IReliableStateManager stateManager;

        private NetworkModelGdaClient nmsGdaClient;

        #region Private Properties
        private Dictionary<DeltaOpType, List<long>> modelChanges;
        private Dictionary<DeltaOpType, List<long>> ModelChanges
        {
            get { return modelChanges ?? (modelChanges = new Dictionary<DeltaOpType, List<long>>()); }
        }

        private Dictionary<long, ISCADAModelPointItem> incomingScadaModel;
        private Dictionary<long, ISCADAModelPointItem> IncomingScadaModel
        {
            get { return incomingScadaModel ?? (incomingScadaModel = new Dictionary<long, ISCADAModelPointItem>()); }
        }

        private Dictionary<PointType, Dictionary<ushort, long>> incomingAddressToGidMap;
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
        #endregion Private Properties

        #region Public Properties
        //TODO: do we need this?
        public bool IsSCADAModelImported { get; private set; }
        
        private ReliableDictionaryAccess<long, ISCADAModelPointItem> currentScadaModel;
        public ReliableDictionaryAccess<long, ISCADAModelPointItem> CurrentScadaModel
        {
            get { return currentScadaModel ?? (currentScadaModel = new ReliableDictionaryAccess<long, ISCADAModelPointItem>(stateManager, ReliableDictionaryNames.GidToPointItemMap)); }
        }

        private ReliableDictionaryAccess<short, Dictionary<ushort, long>> currentAddressToGidMap;
        public ReliableDictionaryAccess<short, Dictionary<ushort, long>> CurrentAddressToGidMap
        {
            get
            {
                return currentAddressToGidMap ?? (currentAddressToGidMap = new ReliableDictionaryAccess<short, Dictionary<ushort, long>>(stateManager, ReliableDictionaryNames.AddressToGidMap)
                {
                    { (short)PointType.ANALOG_INPUT,   new Dictionary<ushort, long>()  },
                    { (short)PointType.ANALOG_OUTPUT,  new Dictionary<ushort, long>()  },
                    { (short)PointType.DIGITAL_INPUT,  new Dictionary<ushort, long>()  },
                    { (short)PointType.DIGITAL_OUTPUT, new Dictionary<ushort, long>()  },
                    { (short)PointType.HR_LONG,        new Dictionary<ushort, long>()  },
                });
            }
        }

        private ReliableDictionaryAccess<long, CommandDescription> commandDescriptionCache;
        public ReliableDictionaryAccess<long, CommandDescription> CommandDescriptionCache
        {
            get { return commandDescriptionCache ?? (commandDescriptionCache = new ReliableDictionaryAccess<long, CommandDescription>(stateManager, ReliableDictionaryNames.CommandDescriptionCache)); }
        }
        #endregion Public Properties

        public ScadaModel(IReliableStateManager stateManager, ModelResourcesDesc modelResourceDesc, EnumDescs enumDescs)
        {
            this.stateManager = stateManager;
            this.modelResourceDesc = modelResourceDesc;
            this.enumDescs = enumDescs;

            this.nmsGdaClient = NetworkModelGdaClient.CreateClient();
        }

        #region ImportScadaModel
        public async Task<bool> ImportModel()
        {
            string message = "Importing analog measurements started...";
            Logger.LogInfo(message);
            Console.WriteLine(message);
            bool analogImportSuccess = await ImportAnalog();

            message = $"Importing analog measurements finished. ['success' value: {analogImportSuccess}]";
            Logger.LogInfo(message);
            Console.WriteLine(message);

            message = "Importing discrete measurements started...";
            Logger.LogInfo(message);
            Console.WriteLine(message);
            bool discreteImportSuccess = await ImportDiscrete();

            message = $"Importing discrete measurements finished. ['success' value: {discreteImportSuccess}]";
            Logger.LogInfo(message);
            Console.WriteLine(message);

            IsSCADAModelImported = analogImportSuccess && discreteImportSuccess;

            //TODO: model confirmation => modbus MU commands
            //if (isSCADAModelImported && SignalIncomingModelConfirmation != null)
            //{
            //    SignalIncomingModelConfirmation.Invoke(new List<long>(CurrentScadaModel.Keys));
            //}

            return IsSCADAModelImported;
        }

        private async Task<bool> ImportAnalog()
        {
            bool success;
            int numberOfResources = 1000;
            List<ModelCode> props = modelResourceDesc.GetAllPropertyIds(ModelCode.ANALOG);

            if (this.nmsGdaClient == null)
            {
                success = false;
                string errMsg = "From ImportAnalog() method: NetworkModelGdaClient is null.";
                Logger.LogWarn(errMsg);

                this.nmsGdaClient = NetworkModelGdaClient.CreateClient();
            }

            try
            {
                int iteratorId = await nmsGdaClient.GetExtentValues(ModelCode.ANALOG, props);
                int resourcesLeft = await nmsGdaClient.IteratorResourcesLeft(iteratorId);

                while (resourcesLeft > 0)
                {
                    List<ResourceDescription> rds = await nmsGdaClient.IteratorNext(numberOfResources, iteratorId);

                    for (int i = 0; i < rds.Count; i++)
                    {
                        if (rds[i] != null)
                        {
                            long gid = rds[i].Id;
                            ModelCode type = modelResourceDesc.GetModelCodeFromId(gid);
                            ISCADAModelPointItem pointItem = new AnalogSCADAModelPointItem(rds[i].Properties, ModelCode.ANALOG, enumDescs);
                            CurrentScadaModel.Add(rds[i].Id, pointItem);
                            CurrentAddressToGidMap[(short)pointItem.RegisterType].Add(pointItem.Address, rds[i].Id);

                            Logger.LogDebug($"ANALOG measurement added to SCADA model [Gid: {gid}, Address: {pointItem.Address}]");
                        }
                    }

                    resourcesLeft = await nmsGdaClient.IteratorResourcesLeft(iteratorId);
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

            return success;
        }

        private async Task<bool> ImportDiscrete()
        {
            bool success;
            int numberOfResources = 1000;
            List<ModelCode> props = modelResourceDesc.GetAllPropertyIds(ModelCode.DISCRETE);

            if (this.nmsGdaClient == null)
            {
                success = false;
                string errMsg = "From ImportDiscrete() method: NetworkModelGdaClient is null.";
                Logger.LogWarn(errMsg);

                this.nmsGdaClient = NetworkModelGdaClient.CreateClient();
            }

            try
            {
                int iteratorId = await nmsGdaClient.GetExtentValues(ModelCode.DISCRETE, props);
                int resourcesLeft = await nmsGdaClient.IteratorResourcesLeft(iteratorId);

                while (resourcesLeft > 0)
                {
                    List<ResourceDescription> rds = await nmsGdaClient.IteratorNext(numberOfResources, iteratorId);

                    for (int i = 0; i < rds.Count; i++)
                    {
                        if (rds[i] != null)
                        {
                            long gid = rds[i].Id;
                            ModelCode type = modelResourceDesc.GetModelCodeFromId(gid);
                            ISCADAModelPointItem pointItem = new DiscreteSCADAModelPointItem(rds[i].Properties, ModelCode.DISCRETE, enumDescs);
                            CurrentScadaModel.Add(gid, pointItem);
                            CurrentAddressToGidMap[(short)pointItem.RegisterType].Add(pointItem.Address, gid);
                            Logger.LogDebug($"DISCRETE measurement added to SCADA model [Gid: {gid}, Address: {pointItem.Address}]");
                        }
                    }

                    resourcesLeft = await nmsGdaClient.IteratorResourcesLeft(iteratorId);
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

            return success;
        }
        #endregion ImportScadaModel

        #region IModelUpdateNotificationContract
        public async Task<bool> NotifyAboutUpdate(Dictionary<DeltaOpType, List<long>> modelChanges)
        {
            this.modelChanges = modelChanges;
            return true;
        }
        #endregion IModelUpdateNotificationContract

        #region ITransactionActorContract
        public async Task<bool> Prepare()
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
                Dictionary<long, ISCADAModelPointItem> incomingPointItems = await CreatePointItemsFromNetworkModelMeasurements();

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

        public Task Commit()
        {
            return Task.Run(() =>
            {
               //todo: currentScadaModel  = IncomingScadaModel;
                incomingScadaModel = null;

                //todo: currentAddressToGidMap = IncomingAddressToGidMap;
                incomingAddressToGidMap = null;

                modelChanges.Clear();
                CommandDescriptionCache.Clear();

                string message = $"Incoming SCADA model is confirmed.";
                Console.WriteLine(message);
                Logger.LogInfo(message);

                //TODO: model confirmation => modbus MU commands
                //SignalIncomingModelConfirmation.Invoke(new List<long>(CurrentScadaModel.Keys));
            });
        }

        public Task Rollback()
        {
            return Task.Run(() =>
            {
                incomingScadaModel = null;
                incomingAddressToGidMap = null;
                modelChanges.Clear();

                string message = $"Incoming SCADA model is rejected.";
                Console.WriteLine(message);
                Logger.LogInfo(message);
            });
        }
        #endregion ITransactionActorContract

        #region Private Methods
        private async Task<Dictionary<long, ISCADAModelPointItem>> CreatePointItemsFromNetworkModelMeasurements()
        {
            Dictionary<long, ISCADAModelPointItem> pointItems = new Dictionary<long, ISCADAModelPointItem>();

            if (this.nmsGdaClient == null)
            {
                string message = "From method CreatePointItemsFromNetworkModelMeasurements(): NetworkModelGdaClient is null.";
                Logger.LogWarn(message);

                this.nmsGdaClient = NetworkModelGdaClient.CreateClient();
            }

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
                    iteratorId = await nmsGdaClient.GetExtentValues(type, props);
                    resourcesLeft = await nmsGdaClient.IteratorResourcesLeft(iteratorId);

                    while (resourcesLeft > 0)
                    {
                        List<ResourceDescription> resources = await nmsGdaClient.IteratorNext(numberOfResources, iteratorId);

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

                        resourcesLeft = await nmsGdaClient.IteratorResourcesLeft(iteratorId);
                    }

                    await nmsGdaClient.IteratorClose(iteratorId);
                }
                catch (Exception ex)
                {
                    string errorMessage = $"CreatePointItemsFromNetworkModelMeasurements failed with error: {ex.Message}";
                    Console.WriteLine(errorMessage);
                    Logger.LogError(errorMessage, ex);
                }
            }

            return pointItems;
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
        #endregion Private Methods
    }
}

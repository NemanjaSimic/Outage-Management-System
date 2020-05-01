﻿using Common.SCADA;
using Microsoft.ServiceFabric.Data;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.Cloud.WcfServiceFabricClients.NMS;
using OMS.Common.DistributedTransactionContracts;
using OMS.Common.NmsContracts.GDA;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts.DataContracts;
using Outage.Common;
using SCADA.ModelProviderImplementation.AlarmStrategies;
using SCADA.ModelProviderImplementation.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SCADA.ModelProviderImplementation
{
    internal sealed class ScadaModel : IModelUpdateNotificationContract, ITransactionActorContract
    {
        private ILogger logger;
        private ILogger Logger { get { return logger ?? (logger = LoggerWrapper.Instance); } }

        private readonly EnumDescs enumDescs;
        private readonly ModelResourcesDesc modelResourceDesc;
        private readonly IReliableStateManager stateManager;
        private readonly ScadaModelPointItemHelper pointItemHelper;
        private readonly AnalogAlarmStrategy analogAlarmStrategy;
        private readonly DiscreteAlarmStrategy discreteAlarmStrategy;

        private NetworkModelGdaClient nmsGdaClient;

        #region Private Properties
        private Dictionary<DeltaOpType, List<long>> modelChanges;
        private Dictionary<DeltaOpType, List<long>> ModelChanges
        {
            get { return modelChanges ?? (modelChanges = new Dictionary<DeltaOpType, List<long>>()); }
        }

        private Dictionary<long, IScadaModelPointItem> incomingScadaModel;
        private Dictionary<long, IScadaModelPointItem> IncomingScadaModel
        {
            get { return incomingScadaModel ?? (incomingScadaModel = new Dictionary<long, IScadaModelPointItem>()); }
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

        private ReliableDictionaryAccess<long, IScadaModelPointItem> currentScadaModel;
        public ReliableDictionaryAccess<long, IScadaModelPointItem> CurrentScadaModel
        {
            get { return currentScadaModel ?? (currentScadaModel = new ReliableDictionaryAccess<long, IScadaModelPointItem>(stateManager, ReliableDictionaryNames.GidToPointItemMap)); }
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
            this.pointItemHelper = new ScadaModelPointItemHelper();
            this.analogAlarmStrategy = new AnalogAlarmStrategy();
            this.discreteAlarmStrategy = new DiscreteAlarmStrategy();

            this.nmsGdaClient = NetworkModelGdaClient.CreateClient();

            InitializeScadaModel();
        }

        private async void InitializeScadaModel(bool isRetry = false)
        {
            try
            {
                if(!await ImportModel())
                {
                    this.nmsGdaClient = NetworkModelGdaClient.CreateClient();
                    if (!await ImportModel(true))
                    {
                        string message = "ScadaModel: failed to ImportModel.";
                        Logger.LogError(message);
                        throw new Exception(message);
                    }
                }
            }
            catch (Exception e)
            {
                if (!isRetry)
                {
                    this.nmsGdaClient = NetworkModelGdaClient.CreateClient();
                    InitializeScadaModel(true);
                }
                else
                {
                    string message = "Exception caught in InitializeScadaModel method.";
                    Logger.LogError(message, e);
                    throw e;
                }
            }
        }

        #region ImportScadaModel
        public async Task<bool> ImportModel(bool isRetry = false)
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

            try
            {
                int iteratorId = await this.nmsGdaClient.GetExtentValues(ModelCode.ANALOG, props);
                int resourcesLeft = await this.nmsGdaClient.IteratorResourcesLeft(iteratorId);

                while (resourcesLeft > 0)
                {
                    List<ResourceDescription> rds = await this.nmsGdaClient.IteratorNext(numberOfResources, iteratorId);

                    for (int i = 0; i < rds.Count; i++)
                    {
                        if (rds[i] != null)
                        {
                            long gid = rds[i].Id;
                            ModelCode type = modelResourceDesc.GetModelCodeFromId(gid);

                            IScadaModelPointItem pointItem = new AnalogPointItem(this.analogAlarmStrategy);
                            pointItemHelper.InitializeAnalogPointItem(pointItem as AnalogPointItem, rds[i].Properties, ModelCode.ANALOG, enumDescs);
                            CurrentScadaModel.Add(rds[i].Id, pointItem);
                            CurrentAddressToGidMap[(short)pointItem.RegisterType].Add(pointItem.Address, rds[i].Id);

                            Logger.LogDebug($"ANALOG measurement added to SCADA model [Gid: {gid}, Address: {pointItem.Address}]");
                        }
                    }

                    resourcesLeft = await this.nmsGdaClient.IteratorResourcesLeft(iteratorId);
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

            try
            {
                int iteratorId = await this.nmsGdaClient.GetExtentValues(ModelCode.DISCRETE, props);
                int resourcesLeft = await this.nmsGdaClient.IteratorResourcesLeft(iteratorId);

                while (resourcesLeft > 0)
                {
                    List<ResourceDescription> rds = await this.nmsGdaClient.IteratorNext(numberOfResources, iteratorId);

                    for (int i = 0; i < rds.Count; i++)
                    {
                        if (rds[i] != null)
                        {
                            long gid = rds[i].Id;
                            ModelCode type = modelResourceDesc.GetModelCodeFromId(gid);

                            IScadaModelPointItem pointItem = new DiscretePointItem(this.discreteAlarmStrategy);
                            pointItemHelper.InitializeDiscretePointItem(pointItem as DiscretePointItem, rds[i].Properties, ModelCode.DISCRETE, enumDescs);
                            CurrentScadaModel.Add(gid, pointItem);
                            CurrentAddressToGidMap[(short)pointItem.RegisterType].Add(pointItem.Address, gid);
                            Logger.LogDebug($"DISCRETE measurement added to SCADA model [Gid: {gid}, Address: {pointItem.Address}]");
                        }
                    }

                    resourcesLeft = await this.nmsGdaClient.IteratorResourcesLeft(iteratorId);
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
                incomingScadaModel = new Dictionary<long, IScadaModelPointItem>(CurrentScadaModel.Count);

                foreach (long gid in CurrentScadaModel.Keys)
                {
                    ModelCode type = modelResourceDesc.GetModelCodeFromId(gid);
                    IScadaModelPointItem pointItem = CurrentScadaModel[gid].Clone();

                    IncomingScadaModel.Add(gid, pointItem);

                    if (!IncomingAddressToGidMap[pointItem.RegisterType].ContainsKey(pointItem.Address))
                    {
                        IncomingAddressToGidMap[pointItem.RegisterType].Add(pointItem.Address, gid);
                    }
                }

                //IMPORT ALL measurements from NMS and create PointItems for them
                Dictionary<long, IScadaModelPointItem> incomingPointItems = await CreatePointItemsFromNetworkModelMeasurements();

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

                        IScadaModelPointItem oldPointItem = IncomingScadaModel[gid];
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

                        IScadaModelPointItem incomingPointItem = incomingPointItems[gid];
                        IScadaModelPointItem oldPointItem = IncomingScadaModel[gid];

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

                        IScadaModelPointItem incomingPointItem = incomingPointItems[gid];

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
        private async Task<Dictionary<long, IScadaModelPointItem>> CreatePointItemsFromNetworkModelMeasurements()
        {
            Dictionary<long, IScadaModelPointItem> pointItems = new Dictionary<long, IScadaModelPointItem>();

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
                    iteratorId = await this.nmsGdaClient.GetExtentValues(type, props);
                    resourcesLeft = await this.nmsGdaClient.IteratorResourcesLeft(iteratorId);

                    while (resourcesLeft > 0)
                    {
                        List<ResourceDescription> resources = await this.nmsGdaClient.IteratorNext(numberOfResources, iteratorId);

                        foreach (ResourceDescription rd in resources)
                        {
                            if (pointItems.ContainsKey(rd.Id))
                            {
                                string message = $"Trying to create point item for resource that already exists in model. Gid: 0x{rd.Id:X16}";
                                Logger.LogError(message);
                                throw new ArgumentException(message);
                            }

                            IScadaModelPointItem point;

                            //change service contract IModelUpdateNotificationContract => change List<long> to Hashset<long> 
                            if (ModelChanges[DeltaOpType.Update].Contains(rd.Id) || ModelChanges[DeltaOpType.Insert].Contains(rd.Id))
                            {
                                point = CreatePointItemFromResource(rd);
                                pointItems.Add(rd.Id, point);
                            }
                        }

                        resourcesLeft = await this.nmsGdaClient.IteratorResourcesLeft(iteratorId);
                    }

                    await this.nmsGdaClient.IteratorClose(iteratorId);
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

        private IScadaModelPointItem CreatePointItemFromResource(ResourceDescription resource)
        {
            long gid = resource.Id;
            ModelCode type = modelResourceDesc.GetModelCodeFromId(gid);

            IScadaModelPointItem pointItem;

            if (type == ModelCode.ANALOG)
            {
                pointItem = new AnalogPointItem(this.analogAlarmStrategy);
                pointItemHelper.InitializeAnalogPointItem(pointItem as AnalogPointItem, resource.Properties, type, enumDescs);
            }
            else if (type == ModelCode.DISCRETE)
            {
                pointItem = new DiscretePointItem(this.discreteAlarmStrategy);
                pointItemHelper.InitializeDiscretePointItem(pointItem as DiscretePointItem, resource.Properties, type, enumDescs);
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
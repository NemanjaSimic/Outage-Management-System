using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Exceptions.SCADA;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.NmsContracts;
using OMS.Common.NmsContracts.GDA;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts.Commanding;
using OMS.Common.ScadaContracts.DataContracts;
using OMS.Common.ScadaContracts.DataContracts.ScadaModelPointItems;
using OMS.Common.TmsContracts;
using OMS.Common.WcfClient.NMS;
using OMS.Common.WcfClient.SCADA;
using SCADA.ModelProviderImplementation.Data;
using SCADA.ModelProviderImplementation.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.ModelProviderImplementation.DistributedTransaction
{
    public class ScadaTransactionActor : ITransactionActorContract
    {
        private readonly string baseLogString;
        private readonly EnumDescs enumDescs;
        private readonly ModelResourcesDesc modelResourceDesc;
        private readonly ScadaModelPointItemHelper pointItemHelper;
        private readonly ReliableDictionaryHelper reliableDictionaryHelper;
        private readonly IReliableStateManager stateManager;

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        #region Reliable Dictionaries
        private bool isCurrentGidToPointItemMapInitialized;
        private bool isIncomingGidToPointItemMapInitialized;
        private bool isCurrentAddressToGidMapInitialized;
        private bool isIncomingAddressToGidMapInitialized;
        private bool isInfoCacheInitialized;
        private bool isModelChangesInitialized;
        private bool isCommandDescriptionCacheInitialized;
        private bool isMeasurementsCacheInitialized;

        private bool ReliableDictionariesInitialized
        {
            get 
            {
                return isCurrentGidToPointItemMapInitialized &&
                       isIncomingGidToPointItemMapInitialized &&
                       isCurrentAddressToGidMapInitialized &&
                       isIncomingAddressToGidMapInitialized &&
                       isInfoCacheInitialized &&
                       isModelChangesInitialized &&
                       isCommandDescriptionCacheInitialized &&
                       isMeasurementsCacheInitialized;
            }
        }

        private ReliableDictionaryAccess<long, IScadaModelPointItem> CurrentGidToPointItemMap { get; set; }
        private ReliableDictionaryAccess<long, IScadaModelPointItem> IncomingGidToPointItemMap { get; set; }
        private ReliableDictionaryAccess<short, Dictionary<ushort, long>> CurrentAddressToGidMap { get; set; }
        private ReliableDictionaryAccess<short, Dictionary<ushort, long>> IncomingAddressToGidMap { get; set; }
        private ReliableDictionaryAccess<string, bool> InfoCache { get; set; }
        private ReliableDictionaryAccess<byte, List<long>> ModelChanges { get; set; }
        private ReliableDictionaryAccess<long, CommandDescription> CommandDescriptionCache { get; set; }
        private ReliableDictionaryAccess<long, ModbusData> MeasurementsCache { get; set; }

        private async void OnStateManagerChangedHandler(object sender, NotifyStateManagerChangedEventArgs e)
        {
            if (e.Action == NotifyStateManagerChangedAction.Add)
            {
                var operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
                string reliableStateName = operation.ReliableState.Name.AbsolutePath;

                if (reliableStateName == ReliableDictionaryNames.GidToPointItemMap)
                {
                    CurrentGidToPointItemMap = await ReliableDictionaryAccess<long, IScadaModelPointItem>.Create(stateManager, ReliableDictionaryNames.GidToPointItemMap);
                    this.isCurrentGidToPointItemMapInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.GidToPointItemMap}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if(reliableStateName == ReliableDictionaryNames.IncomingGidToPointItemMap)
                {
                    IncomingGidToPointItemMap = await ReliableDictionaryAccess<long, IScadaModelPointItem>.Create(stateManager, ReliableDictionaryNames.IncomingGidToPointItemMap);
                    this.isIncomingGidToPointItemMapInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.IncomingGidToPointItemMap}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.AddressToGidMap)
                {
                    CurrentAddressToGidMap = await ReliableDictionaryAccess<short, Dictionary<ushort, long>>.Create(stateManager, ReliableDictionaryNames.AddressToGidMap);
                    this.isCurrentAddressToGidMapInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.AddressToGidMap}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.IncomingAddressToGidMap)
                {
                    IncomingAddressToGidMap = await ReliableDictionaryAccess<short, Dictionary<ushort, long>>.Create(stateManager, ReliableDictionaryNames.IncomingAddressToGidMap);
                    this.isIncomingAddressToGidMapInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.IncomingAddressToGidMap}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.InfoCache)
                {
                    InfoCache = await ReliableDictionaryAccess<string, bool>.Create(stateManager, ReliableDictionaryNames.InfoCache);
                    isInfoCacheInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.InfoCache}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.ModelChanges)
                {
                    ModelChanges = await ReliableDictionaryAccess<byte, List<long>>.Create(stateManager, ReliableDictionaryNames.ModelChanges);
                    this.isModelChangesInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.ModelChanges}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.CommandDescriptionCache)
                {
                    CommandDescriptionCache = await ReliableDictionaryAccess<long, CommandDescription>.Create(stateManager, ReliableDictionaryNames.CommandDescriptionCache);
                    this.isCommandDescriptionCacheInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.CommandDescriptionCache}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.MeasurementsCache)
                {
                    MeasurementsCache = await ReliableDictionaryAccess<long, ModbusData>.Create(stateManager, ReliableDictionaryNames.MeasurementsCache);
                    this.isMeasurementsCacheInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.MeasurementsCache}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
            }
        }
        #endregion Reliable Dictionaries

        public ScadaTransactionActor(IReliableStateManager stateManager, ModelResourcesDesc modelResourceDesc, EnumDescs enumDescs)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            this.isCurrentGidToPointItemMapInitialized = false;
            this.isIncomingGidToPointItemMapInitialized = false;
            this.isCurrentAddressToGidMapInitialized = false;
            this.isIncomingAddressToGidMapInitialized = false;
            this.isInfoCacheInitialized = false;
            this.isModelChangesInitialized = false;
            this.isCommandDescriptionCacheInitialized = false;

            this.stateManager = stateManager;
            this.stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;

            this.modelResourceDesc = modelResourceDesc;
            this.enumDescs = enumDescs;
            this.pointItemHelper = new ScadaModelPointItemHelper();
            this.reliableDictionaryHelper = new ReliableDictionaryHelper();
        }

        #region ITransactionActorContract
        public async Task<bool> Prepare()
        {
            bool success;

            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            try
            {
                //INIT INCOMING SCADA MODEL with current model values
                await InitializeIncomingScadaModel();

                //IMPORT ALL measurements from NMS and create PointItems for them
                var enumerableModelChanges = await ModelChanges.GetEnumerableDictionaryAsync();
                Dictionary<long, IScadaModelPointItem> incomingPointItems = await CreatePointItemsFromNetworkModelMeasurements(enumerableModelChanges);

                //ORDER IS IMPORTANT due to IncomingAddressToGidMap validity: DELETE => UPDATE => INSERT
                var orderOfOperations = new List<DeltaOpType>() { DeltaOpType.Delete, DeltaOpType.Update, DeltaOpType.Insert };

                foreach (var operation in orderOfOperations)
                {
                    foreach(long gid in enumerableModelChanges[(byte)operation])
                    {
                        ModelCode type = modelResourceDesc.GetModelCodeFromId(gid);
                        if (type != ModelCode.ANALOG && type != ModelCode.DISCRETE)
                        {
                            continue;
                        }

                        if (operation == DeltaOpType.Delete)
                        {
                            await HandleDeleteOperation(gid);
                        }
                        else if (operation == DeltaOpType.Update)
                        {
                            IScadaModelPointItem incomingPointItem = incomingPointItems[gid];
                            await HandleUpdateOperation(incomingPointItem, gid);
                        }
                        else if (operation == DeltaOpType.Insert)
                        {
                            IScadaModelPointItem incomingPointItem = incomingPointItems[gid];
                            await HandleInsertOperation(incomingPointItem, gid);
                        }
                    }
                }

                success = await CheckSuccessiveAddressCondition();
;
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} Prepare => Exception: {e.Message}";
                Logger.LogError(errorMessage, e);
                success = false;
            }

            return success;
        }

        public async Task Commit()
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            try
            {
                await reliableDictionaryHelper.TryCopyToReliableDictionary<long, IScadaModelPointItem>(ReliableDictionaryNames.IncomingGidToPointItemMap, ReliableDictionaryNames.GidToPointItemMap, this.stateManager);
                await reliableDictionaryHelper.TryCopyToReliableDictionary<short, Dictionary<ushort, long>>(ReliableDictionaryNames.IncomingAddressToGidMap, ReliableDictionaryNames.AddressToGidMap, this.stateManager);

                await IncomingGidToPointItemMap.ClearAsync();
                await IncomingAddressToGidMap.ClearAsync();
                await ModelChanges.ClearAsync();

                await CommandDescriptionCache.ClearAsync();
                await MeasurementsCache.ClearAsync();

                string message = $"{baseLogString} Commit => Incoming SCADA model is confirmed.";
                Logger.LogInformation(message);

                await SendModelUpdateCommands();

                await LogAllReliableCollections();
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} Commit => Exception: {e.Message}";
                Logger.LogError(errorMessage, e);
            }
        }

        public async Task Rollback()
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            try
            {
                await IncomingGidToPointItemMap.ClearAsync();
                await IncomingAddressToGidMap.ClearAsync();
                await ModelChanges.ClearAsync();

                string message = $"{baseLogString} Rollback => Incoming SCADA model is rejected.";
                Logger.LogInformation(message);

                await LogAllReliableCollections();
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} Rollback => Exception: {e.Message}";
                Logger.LogError(errorMessage, e);
            }
        }
        #endregion ITransactionActorContract

        #region Private Methods
        private async Task InitializeIncomingScadaModel()
        {
            var enumerableCurrentGidToPointItemMap = await CurrentGidToPointItemMap.GetEnumerableDictionaryAsync();

            foreach (long gid in enumerableCurrentGidToPointItemMap.Keys)
            {
                //TODO: to tasks, await all

                IScadaModelPointItem pointItem = enumerableCurrentGidToPointItemMap[gid].Clone();

                await IncomingGidToPointItemMap.SetAsync(gid, pointItem);

                short registerType = (short)pointItem.RegisterType;
                if (!(await IncomingAddressToGidMap.ContainsKeyAsync(registerType)))
                {
                    await IncomingAddressToGidMap.SetAsync(registerType, new Dictionary<ushort, long>());
                }

                var addressToGidDictionaryResult = await IncomingAddressToGidMap.TryGetValueAsync(registerType);
                if (!addressToGidDictionaryResult.HasValue)
                {
                    string message = $"{baseLogString} InitializeIncomingScadaModel => reliable collection '{ReliableDictionaryNames.IncomingAddressToGidMap}' is not initialized properly.";
                    Logger.LogError(message);
                    throw new InternalSCADAServiceException(message);
                }

                var addressToGidDictionary = addressToGidDictionaryResult.Value;

                if (addressToGidDictionary.ContainsKey(pointItem.Address))
                {
                    string message = $"{baseLogString} InitializeIncomingScadaModel => SCADA model is invalid => Address: {pointItem.Address} (RegType: {registerType}) belongs to more than one entity.";
                    Logger.LogError(message);
                    throw new InternalSCADAServiceException(message);
                }

                addressToGidDictionary.Add(pointItem.Address, pointItem.Gid);
                await IncomingAddressToGidMap.SetAsync(registerType, addressToGidDictionary);

                string debugMessage = $"{baseLogString} InitializeIncomingScadaModel => measurement added to Incoming SCADA model [Gid: 0x{gid:X16}, Address: {pointItem.Address}]";
                Logger.LogDebug(debugMessage);
            }
        }

        private async Task<Dictionary<long, IScadaModelPointItem>> CreatePointItemsFromNetworkModelMeasurements(Dictionary<byte, List<long>> modelChanges)
        {
            INetworkModelGDAContract nmsGdaClient = NetworkModelGdaClient.CreateClient();

            Dictionary<long, IScadaModelPointItem> pointItems = new Dictionary<long, IScadaModelPointItem>();

            int iteratorId;
            int resourcesLeft;
            int numberOfResources = 10000;

            List<ModelCode> props;

            //TOOD: change service contract IModelUpdateNotificationContract to receive types of all changed elements from NMS 
            var changedTypesHashSet = new HashSet<ModelCode>();

            foreach (var gids in modelChanges.Values)
            {
                foreach (var gid in gids)
                {
                    ModelCode type = modelResourceDesc.GetModelCodeFromId(gid);
                    changedTypesHashSet.Add(type);
                }
            }

            foreach (ModelCode type in changedTypesHashSet)
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
                                string message = $"{baseLogString} CreatePointItemsFromNetworkModelMeasurements => Trying to create point item for resource that already exists in model. Gid: 0x{rd.Id:X16}";
                                Logger.LogError(message);
                                throw new ArgumentException(message);
                            }

                            IScadaModelPointItem point;

                            //change service contract IModelUpdateNotificationContract => change List<long> to Hashset<long> 
                            if (modelChanges[(byte)DeltaOpType.Update].Contains(rd.Id) || modelChanges[(byte)DeltaOpType.Insert].Contains(rd.Id))
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
                    string errorMessage = $"{baseLogString} CreatePointItemsFromNetworkModelMeasurements => Failed with error: {ex.Message}";
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
                pointItem = new AnalogPointItem(AlarmConfigDataHelper.GetAlarmConfigData());
                pointItemHelper.InitializeAnalogPointItem(pointItem as AnalogPointItem, resource.Properties, type, enumDescs);
            }
            else if (type == ModelCode.DISCRETE)
            {
                pointItem = new DiscretePointItem(AlarmConfigDataHelper.GetAlarmConfigData());
                pointItemHelper.InitializeDiscretePointItem(pointItem as DiscretePointItem, resource.Properties, type, enumDescs);
            }
            else
            {
                string errMessage = $"{baseLogString} CreatePointItemFromResource => ResourceDescription type is neither analog nor digital. Type: {type}.";
                Logger.LogWarning(errMessage);
                pointItem = null;
            }

            return pointItem;
        }

        private async Task HandleDeleteOperation(long gid)
        {
            var pointItemResult = await IncomingGidToPointItemMap.TryGetValueAsync(gid);
            if (!pointItemResult.HasValue)
            {
                string errorMessage = $"{baseLogString} HandleDeleteOperation => Model update data in fault state. Deleting entity with gid: {gid:X16}, that does not exists in IncomingGidToPointItemMap.";
                Logger.LogError(errorMessage);
                throw new ArgumentException(errorMessage);
            }

            IScadaModelPointItem oldPointItem = pointItemResult.Value;

            Dictionary<ushort, long> incomingAddressToGidMapDictionary;
            var type = (short)oldPointItem.RegisterType;

            var addressToGidMapResult = await IncomingAddressToGidMap.TryGetValueAsync(type);

            if (!addressToGidMapResult.HasValue)
            {
                incomingAddressToGidMapDictionary = new Dictionary<ushort, long>();
            }
            else
            {
                incomingAddressToGidMapDictionary = addressToGidMapResult.Value;
            }

            if (!incomingAddressToGidMapDictionary.ContainsKey(oldPointItem.Address))
            {
                string message = $"{baseLogString} HandleDeleteOperation => Model update data in fault state. Deleting point with address: {oldPointItem.Address}, that does not exists in IncomingAddressToGidMap.";
                Logger.LogError(message);
                throw new ArgumentException(message);
            }

            //LOGIC
            await IncomingGidToPointItemMap.TryRemoveAsync(gid);

            incomingAddressToGidMapDictionary.Remove(oldPointItem.Address);
            await IncomingAddressToGidMap.SetAsync(type, incomingAddressToGidMapDictionary);

            string debugMessage = $"{baseLogString} HandleDeleteOperation => SUCCESSFULLY deleted point from IncomingGidToPointItemMap and IncomingAddressToGidMap. Gid: 0x{oldPointItem.Gid:X16}, Address: {oldPointItem.Address}";
            Logger.LogDebug(debugMessage);
        }

        private async Task HandleUpdateOperation(IScadaModelPointItem incomingPointItem, long gid)
        {
            var pointItemResult = await IncomingGidToPointItemMap.TryGetValueAsync(gid);
            if (!pointItemResult.HasValue)
            {
                string errorMessage = $"{baseLogString} HandleUpdateOperation => Model update data in fault state. Updating entity with gid: {gid:X16}, that does not exists in IncomingGidToPointItemMap.";
                Logger.LogError(errorMessage);
                throw new ArgumentException(errorMessage);
            }

            IScadaModelPointItem oldPointItem = pointItemResult.Value;

            var addressToGidMapResult = await IncomingAddressToGidMap.TryGetValueAsync((short)oldPointItem.RegisterType);
            if (!addressToGidMapResult.HasValue)
            {
                string errorMessage = $"{baseLogString} HandleUpdateOperation => Model update data in fault state. Updating entity with gid: {gid:X16}, {oldPointItem.RegisterType} registry type does not exist in incoming IncomingAddressToGidMap.";
                Logger.LogError(errorMessage);
                throw new ArgumentException(errorMessage);
            }

            var incomingAddressToGidMapDictionary = addressToGidMapResult.Value;
            if (!incomingAddressToGidMapDictionary.ContainsKey(oldPointItem.Address))
            {
                string message = $"{baseLogString} HandleUpdateOperation => Model update data in fault state. Updating point with address: {oldPointItem.Address}, that does not exists in IncomingAddressToGidMap.";
                Logger.LogError(message);
                throw new ArgumentException(message);
            }

            if (oldPointItem.Address != incomingPointItem.Address && incomingAddressToGidMapDictionary.ContainsKey(incomingPointItem.Address))
            {
                string message = $"{baseLogString} HandleUpdateOperation => Model update data in fault state. Trying to add point with address: {incomingPointItem.Address}, that already exists in IncomingAddressToGidMap.";
                Logger.LogError(message);
                throw new ArgumentException(message);
            }

            string appendMessage = ""; string oldAddressText = "";

            //LOGIC
            await IncomingGidToPointItemMap.SetAsync(gid, incomingPointItem);

            if (oldPointItem.Address != incomingPointItem.Address)
            {
                incomingAddressToGidMapDictionary.Remove(oldPointItem.Address);
                incomingAddressToGidMapDictionary.Add(incomingPointItem.Address, gid);

                await IncomingAddressToGidMap.SetAsync((short)oldPointItem.RegisterType, incomingAddressToGidMapDictionary);
                appendMessage = " and IncomingAddressToGidMap";
                oldAddressText = $", Old address: {oldPointItem.Address}";
            }

            string debugMessage = $"{baseLogString} HandleUpdateOperation => SUCCESSFULLY updated point from IncomingGidToPointItemMap{appendMessage}. Gid: 0x{incomingPointItem.Gid:X16}, Address: {incomingPointItem.Address}{oldAddressText}";
            Logger.LogDebug(debugMessage);
        }

        private async Task HandleInsertOperation(IScadaModelPointItem incomingPointItem, long gid)
        {
            var pointItemResult = await IncomingGidToPointItemMap.TryGetValueAsync(gid);
            if (pointItemResult.HasValue)
            {
                string errorMessage = $"{baseLogString} HandleInsertOperation => Model update data in fault state. Inserting entity with gid: {gid:X16}, that already exists in IncomingGidToPointItemMap.";
                Logger.LogError(errorMessage);
                throw new ArgumentException(errorMessage);
            }

            Dictionary<ushort, long> incomingAddressToGidMapDictionary;
            var type = (short)incomingPointItem.RegisterType;
            var addressToGidMapResult = await IncomingAddressToGidMap.TryGetValueAsync(type);

            if (!addressToGidMapResult.HasValue)
            {
                incomingAddressToGidMapDictionary = new Dictionary<ushort, long>();
            }
            else
            {
                incomingAddressToGidMapDictionary = addressToGidMapResult.Value;
            }

            if (incomingAddressToGidMapDictionary.ContainsKey(incomingPointItem.Address))
            {
                string message = $"{baseLogString} HandleInsertOperation => Model update data in fault state. Inserting entity with address: {incomingPointItem.Address}, that already exists in IncomingAddressToGidMap.";
                Logger.LogError(message);
                throw new ArgumentException(message);
            }

            //LOGIC
            await IncomingGidToPointItemMap.SetAsync(gid, incomingPointItem);

            incomingAddressToGidMapDictionary.Add(incomingPointItem.Address, gid);
            await IncomingAddressToGidMap.SetAsync(type, incomingAddressToGidMapDictionary);

            string debugMessage = $"{baseLogString} HandleInsertOperation => SUCCESSFULLY inserted point into IncomingGidToPointItemMap and IncomingAddressToGidMap. Gid: 0x{incomingPointItem.Gid:X16}, Address: {incomingPointItem.Address}";
            Logger.LogDebug(debugMessage);
        }

        private async Task<bool> CheckSuccessiveAddressCondition()
        {
            bool condition = true;

            var addressToGidMapDictionary = await IncomingAddressToGidMap.GetEnumerableDictionaryAsync();

            foreach(var type in addressToGidMapDictionary.Keys)
            {
                var addressToGidMap = addressToGidMapDictionary[type];

                for (ushort address = 1; address <= addressToGidMap.Values.Count; address++)
                {
                    condition = addressToGidMap.ContainsKey(address);

                    if (condition == false)
                    {
                        string errorMessage = $"{baseLogString} CheckSuccessiveAddressCondition => Addresses of {(PointType)type} measurements are not successive. Probably a problem with cim/xml.";
                        Logger.LogError(errorMessage);
                        break;
                    }
                }

                if(condition == false)
                {
                    break;
                }
            }

            return condition;
        }

        private async Task SendModelUpdateCommands()
        {
            IScadaCommandingContract scadaCommandingClient = ScadaCommandingClient.CreateClient();
            var enumerableAddressToGidMapResult = await CurrentAddressToGidMap.GetEnumerableDictionaryAsync();

            var tasks = new List<Task>()
            {
                Task.Run(async () =>
                {
                    var key = (short)PointType.ANALOG_OUTPUT;
                    if(!enumerableAddressToGidMapResult.ContainsKey(key))
                    {
                        return;
                    }

                    var analogItemsAddressToGidMap = enumerableAddressToGidMapResult[key];
                    var analogCommandingValues = new Dictionary<long, float>(analogItemsAddressToGidMap.Count);

                    foreach (long gid in analogItemsAddressToGidMap.Values)
                    {
                        var result = await CurrentGidToPointItemMap.TryGetValueAsync(gid);
                        var analogPointItem = result.Value as IAnalogPointItem;

                        analogCommandingValues.Add(gid, analogPointItem.CurrentEguValue);
                    }

                    if(analogCommandingValues.Count > 0)
                    {
                        await scadaCommandingClient.SendMultipleAnalogCommand(analogCommandingValues, CommandOriginType.MODEL_UPDATE_COMMAND);
                    }
                }),

                Task.Run(async () =>
                {
                    var key = (short)PointType.DIGITAL_OUTPUT;
                    if(!enumerableAddressToGidMapResult.ContainsKey(key))
                    {
                        return;
                    }

                    var discreteItemsAddressToGidMap = enumerableAddressToGidMapResult[key];
                    var discreteCommandingValues = new Dictionary<long, ushort>(discreteItemsAddressToGidMap.Count);

                    foreach (long gid in discreteItemsAddressToGidMap.Values)
                    {
                        var result = await CurrentGidToPointItemMap.TryGetValueAsync(gid);
                        var discretePointItem = result.Value as IDiscretePointItem;

                        discreteCommandingValues.Add(gid, discretePointItem.CurrentValue);
                    }

                    if(discreteCommandingValues.Count > 0)
                    {
                        await scadaCommandingClient.SendMultipleDiscreteCommand(discreteCommandingValues, CommandOriginType.MODEL_UPDATE_COMMAND);
                    }
                }),
            };

            Task.WaitAll(tasks.ToArray());
        }

        private async Task LogAllReliableCollections()
        {
            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Reliable Collections");

            sb.AppendLine("CurrentGidToPointItemMap =>");
            var currentGidToPointItemMap = await CurrentGidToPointItemMap.GetEnumerableDictionaryAsync();
            foreach(var element in currentGidToPointItemMap)
            {
                sb.AppendLine($"Key => {element.Key}, Value => Gid: 0x{element.Value.Gid:X16}, Address: {element.Value.Address}, Name: {element.Value.Name}, RegisterType: {element.Value.RegisterType}, Alarm: {element.Value.Alarm}, Initialized: {element.Value.Initialized}");
            }
            sb.AppendLine();

            sb.AppendLine("IncomingGidToPointItemMap =>");
            var incomingGidToPointItemMap = await IncomingGidToPointItemMap.GetEnumerableDictionaryAsync();
            foreach (var element in incomingGidToPointItemMap)
            {
                sb.AppendLine($"Key => {element.Key}, Value => Gid: 0x{element.Value.Gid:X16}, Address: {element.Value.Address}, Name: {element.Value.Name}, RegisterType: {element.Value.RegisterType}, Alarm: {element.Value.Alarm}, Initialized: {element.Value.Initialized}");
            }
            sb.AppendLine();

            sb.AppendLine("CurrentAddressToGidMap =>");
            var currentAddressToGidMap = await CurrentAddressToGidMap.GetEnumerableDictionaryAsync();
            foreach (var element in currentAddressToGidMap)
            {
                sb.AppendLine($"Key => {element.Key}, Value => Dictionary Count: {element.Value.Count}");
            }
            sb.AppendLine();

            sb.AppendLine("IncomingAddressToGidMap =>");
            var incomingAddressToGidMap = await IncomingAddressToGidMap.GetEnumerableDictionaryAsync();
            foreach (var element in incomingAddressToGidMap)
            {
                sb.AppendLine($"Key => {element.Key}, Value => Dictionary Count: {element.Value.Count}");
            }
            sb.AppendLine();

            sb.AppendLine("InfoCache =>");
            var infoCache = await InfoCache.GetEnumerableDictionaryAsync();
            foreach (var element in infoCache)
            {
                sb.AppendLine($"Key => {element.Key}, Value => {element.Value}");
            }
            sb.AppendLine();

            sb.AppendLine("ModelChanges =>");
            var modelChanges = await ModelChanges.GetEnumerableDictionaryAsync();
            foreach (var element in modelChanges)
            {
                sb.AppendLine($"Key => {element.Key}, Value => List Count: {element.Value.Count}");
            }
            sb.AppendLine();

            sb.AppendLine("MeasurementsCache =>");
            var measurementsCache = await MeasurementsCache.GetEnumerableDictionaryAsync();
            foreach (var element in measurementsCache)
            {
                sb.AppendLine($"Key => {element.Key}, Value => MeasurementGid: {element.Value.MeasurementGid:X16}, Alarm: {element.Value.Alarm}, CommandOrigin: {element.Value.CommandOrigin}");
            }
            sb.AppendLine();

            sb.AppendLine("CommandDescriptionCache =>");
            var commandDescriptionCache = await CommandDescriptionCache.GetEnumerableDictionaryAsync();
            foreach (var element in commandDescriptionCache)
            {
                sb.AppendLine($"Key => {element.Key}, Value => Gid: {element.Value.Gid:X16}, Address: {element.Value.Address}, Value: {element.Value.Value}, CommandOrigin: {element.Value.CommandOrigin}");
            }
            sb.AppendLine();

            Logger.LogDebug($"{baseLogString} LogAllReliableCollections => {sb}");
        }
        #endregion Private Methods
    }
}

using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Exceptions.SCADA;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.DistributedTransactionContracts;
using OMS.Common.NmsContracts;
using OMS.Common.NmsContracts.GDA;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts.DataContracts;
using OMS.Common.ScadaContracts.DataContracts.ScadaModelPointItems;
using OMS.Common.WcfClient.NMS;
using OMS.Common.WcfClient.SCADA;
using SCADA.ModelProviderImplementation.Data;
using SCADA.ModelProviderImplementation.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.Threading.Tasks;

namespace SCADA.ModelProviderImplementation
{
    public sealed class ScadaModel : IModelUpdateNotificationContract, ITransactionActorContract
    {
        private readonly string baseLogString;
        private readonly EnumDescs enumDescs;
        private readonly ModelResourcesDesc modelResourceDesc;
        private readonly IReliableStateManager stateManager;
        private readonly ScadaModelPointItemHelper pointItemHelper;

        private NetworkModelGdaClient nmsGdaClient;
        private ScadaCommandingClient scadaCommandingClient;

        #region Private Properties
        private bool isModelImported;
        private bool isGidToPointItemMapInitialized;
        private bool isAddressToGidMapInitialized;
        private bool isCommandDescriptionCacheInitialized;
        private bool isInfoCacheInitialized;
        private bool ReliableDictionariesInitialized
        {
            get {   return isGidToPointItemMapInitialized && isAddressToGidMapInitialized && isCommandDescriptionCacheInitialized && isInfoCacheInitialized; }
        }

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        private Dictionary<DeltaOpType, List<long>> modelChanges;
        private Dictionary<DeltaOpType, List<long>> ModelChanges
        {
            get { return modelChanges ?? (modelChanges = new Dictionary<DeltaOpType, List<long>>()); }
        }

        private Dictionary<long, IScadaModelPointItem> incomingGidToPointItemMap;
        private Dictionary<long, IScadaModelPointItem> IncomingScadaModel
        {
            get { return incomingGidToPointItemMap ?? (incomingGidToPointItemMap = new Dictionary<long, IScadaModelPointItem>()); }
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
        private ReliableDictionaryAccess<long, IScadaModelPointItem> currentGidToPointItemMap;
        public ReliableDictionaryAccess<long, IScadaModelPointItem> CurrentGidToPointItemMap
        {
            get { return currentGidToPointItemMap; }
        }

        private ReliableDictionaryAccess<short, Dictionary<ushort, long>> currentAddressToGidMap;
        public ReliableDictionaryAccess<short, Dictionary<ushort, long>> CurrentAddressToGidMap
        {
            get { return currentAddressToGidMap; }
        }

        private ReliableDictionaryAccess<long, CommandDescription> commandDescriptionCache;
        public ReliableDictionaryAccess<long, CommandDescription> CommandDescriptionCache
        {
            get { return commandDescriptionCache; }
        }

        private ReliableDictionaryAccess<string, bool> infoCache;
        public ReliableDictionaryAccess<string, bool> InfoCache
        {
            get { return infoCache; }
        }
        #endregion Public Properties

        public ScadaModel(IReliableStateManager stateManager, ModelResourcesDesc modelResourceDesc, EnumDescs enumDescs)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            this.stateManager = stateManager;
            this.modelResourceDesc = modelResourceDesc;
            this.enumDescs = enumDescs;
            this.pointItemHelper = new ScadaModelPointItemHelper();

            this.nmsGdaClient = NetworkModelGdaClient.CreateClient();
            this.scadaCommandingClient = ScadaCommandingClient.CreateClient();

            this.isGidToPointItemMapInitialized = false;
            this.isAddressToGidMapInitialized = false;
            this.isCommandDescriptionCacheInitialized = false;
            this.isInfoCacheInitialized = false;

            stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;
        }

        private async void OnStateManagerChangedHandler(object sender, NotifyStateManagerChangedEventArgs e)
        {
            if(e.Action == NotifyStateManagerChangedAction.Add)
            {
                var operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
                string reliableStateName = operation.ReliableState.Name.AbsolutePath;

                if (reliableStateName == ReliableDictionaryNames.GidToPointItemMap)
                {
                    //_ = CurrentGidToPointItemMap;
                    currentGidToPointItemMap = await ReliableDictionaryAccess<long, IScadaModelPointItem>.Create(stateManager, ReliableDictionaryNames.GidToPointItemMap);
                    this.isGidToPointItemMapInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.GidToPointItemMap}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if(reliableStateName == ReliableDictionaryNames.AddressToGidMap)
                {
                    //_ = CurrentAddressToGidMap;
                    currentAddressToGidMap = await ReliableDictionaryAccess<short, Dictionary<ushort, long>>.Create(stateManager, ReliableDictionaryNames.AddressToGidMap);
                    await currentAddressToGidMap.SetAsync((short)PointType.ANALOG_INPUT, new Dictionary<ushort, long>());
                    await currentAddressToGidMap.SetAsync((short)PointType.ANALOG_OUTPUT, new Dictionary<ushort, long>());
                    await currentAddressToGidMap.SetAsync((short)PointType.DIGITAL_INPUT, new Dictionary<ushort, long>());
                    await currentAddressToGidMap.SetAsync((short)PointType.DIGITAL_OUTPUT, new Dictionary<ushort, long>());
                    await currentAddressToGidMap.SetAsync((short)PointType.HR_LONG, new Dictionary<ushort, long>());
                    this.isAddressToGidMapInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.AddressToGidMap}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if(reliableStateName == ReliableDictionaryNames.CommandDescriptionCache)
                {
                    //_ = CommandDescriptionCache;
                    commandDescriptionCache = await ReliableDictionaryAccess<long, CommandDescription>.Create(stateManager, ReliableDictionaryNames.CommandDescriptionCache);
                    this.isCommandDescriptionCacheInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.CommandDescriptionCache}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.InfoCache)
                {
                    //_ = InfoCache;
                    infoCache = await ReliableDictionaryAccess<string, bool>.Create(stateManager, ReliableDictionaryNames.InfoCache);
                    isInfoCacheInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.InfoCache}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
            }
            //else if(e.Action == UPDATE, what if?)
        }

        public async Task InitializeScadaModel(bool isRetry = false)
        {
            string isRetryString = isRetry ? "yes" : "no";
            string verboseMessage = $"{baseLogString} InitializeScadaModel method called, isRetry: {isRetryString}.";
            Logger.LogVerbose(verboseMessage);

            while (!ReliableDictionariesInitialized)
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            try
            {
                isModelImported = await ImportModel();
                await InfoCache.SetAsync("IsScadaModelImported", isModelImported);

                if (!isModelImported)
                {
                    string message = $"{baseLogString} InitializeScadaModel => failed to import model";
                    Logger.LogWarning(message);

                    await Task.Delay(2000);
                    await InitializeScadaModel(true);

                    //TODO: neka ozbiljnija retry logiga
                    //throw new Exception($"{baseLogString} InitializeScadaModel => failed to import model");
                }

                await SendModelUpdateCommands();
            }
            catch (CommunicationObjectFaultedException e)
            {
                string errorMessage = $"{baseLogString} InitializeScadaModel => CommunicationObjectFaultedException caught.";
                Logger.LogError(errorMessage, e);

                await Task.Delay(2000);

                this.nmsGdaClient = NetworkModelGdaClient.CreateClient();
                this.scadaCommandingClient = ScadaCommandingClient.CreateClient();
                await InitializeScadaModel(true);
                //todo: different logic on multiple rety?
            }
            catch (Exception e)
            {
                string errorMessage = $"{baseLogString} InitializeScadaModel => Exception caught.";
                Logger.LogError(errorMessage, e);
                throw e;
            }
        }

        #region ImportScadaModel
        public async Task<bool> ImportModel(bool isRetry = false)
        {
            bool success;

            await CurrentGidToPointItemMap.ClearAsync();

            var enumerableCurrentAddressToGidMap = await CurrentAddressToGidMap.GetEnumerableDictionaryAsync();
            foreach (var key in enumerableCurrentAddressToGidMap.Keys)
            {
                var dictionary = enumerableCurrentAddressToGidMap[key];
                dictionary.Clear();

                await CurrentAddressToGidMap.SetAsync(key, dictionary);
            }

            string message = $"{baseLogString} ImportModel => Importing analog measurements started...";
            Logger.LogInformation(message);

            bool analogImportSuccess = await ImportAnalog();

            message = $"{baseLogString} ImportModel =>Importing analog measurements finished. ['success' value: {analogImportSuccess}]";
            Logger.LogInformation(message);

            message = $"{baseLogString} ImportModel => Importing discrete measurements started...";
            Logger.LogInformation(message);

            bool discreteImportSuccess = await ImportDiscrete();

            message = $"{baseLogString} ImportModel => Importing discrete measurements finished. ['success' value: {discreteImportSuccess}]";
            Logger.LogInformation(message);

            success = analogImportSuccess && discreteImportSuccess;

            if(!success)
            {
                await CurrentGidToPointItemMap.ClearAsync();

                enumerableCurrentAddressToGidMap = await CurrentAddressToGidMap.GetEnumerableDictionaryAsync();
                foreach (var key in enumerableCurrentAddressToGidMap.Keys)
                {
                    var dictionary = enumerableCurrentAddressToGidMap[key];
                    dictionary.Clear();

                    await CurrentAddressToGidMap.SetAsync(key, dictionary);
                }
            }

            return success;
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
                        if (rds[i] == null)
                        {
                            continue;
                        }

                        long gid = rds[i].Id;
                        ModelCode type = modelResourceDesc.GetModelCodeFromId(gid);

                        AnalogPointItem analogPoint = new AnalogPointItem(AlarmConfigDataHelper.GetAlarmConfigData());

                        string debugMessage = $"{baseLogString} ImportAnalog => Before Initialization => Gid: 0x{analogPoint.Gid:X16}, Address: {analogPoint.Address}, CurrentRawValue: {analogPoint.CurrentRawValue}, Alarm: {analogPoint.Alarm}, ScalingFactor: {analogPoint.ScalingFactor}, Deviation: {analogPoint.Deviation}, MinRawValue: {analogPoint.MinRawValue}, MaxRawValue: {analogPoint.MaxRawValue}, NormalValue: {analogPoint.NormalValue}, RegisterType: {analogPoint.RegisterType}, Name: {analogPoint.Name}, Initialized: {analogPoint.Initialized}";
                        Logger.LogDebug(debugMessage);

                        pointItemHelper.InitializeAnalogPointItem(analogPoint, rds[i].Properties, ModelCode.ANALOG, enumDescs);

                        debugMessage = $"{baseLogString} ImportAnalog => After Initialization => Gid: 0x{analogPoint.Gid:X16}, Address: {analogPoint.Address}, CurrentRawValue: {analogPoint.CurrentRawValue}, Alarm: {analogPoint.Alarm}, ScalingFactor: {analogPoint.ScalingFactor}, Deviation: {analogPoint.Deviation}, MinRawValue: {analogPoint.MinRawValue}, MaxRawValue: {analogPoint.MaxRawValue}, NormalValue: {analogPoint.NormalValue}, RegisterType: {analogPoint.RegisterType}, Name: {analogPoint.Name}, Initialized: {analogPoint.Initialized}";
                        Logger.LogDebug(debugMessage);

                        if(await CurrentGidToPointItemMap.ContainsKeyAsync(gid))
                        {
                            string errorMessage = $"{baseLogString} ImportAnalog => SCADA model is invalid => Gid: 0x{gid:16} belongs to more than one entity.";
                            Logger.LogError(errorMessage);
                            throw new InternalSCADAServiceException(errorMessage);
                        }

                        await CurrentGidToPointItemMap.SetAsync(gid, analogPoint);

#if DEBUG
                        var pointItemResult = await CurrentGidToPointItemMap.TryGetValueAsync(gid);
                        if(pointItemResult.HasValue)
                        {
                            AnalogPointItem controlPointItem = pointItemResult.Value as AnalogPointItem;
                            debugMessage = $"{baseLogString} ImportAnalog => Control after CurrentGidToPointItemMap.SetAsync => Gid: 0x{controlPointItem.Gid:X16}, Address: {controlPointItem.Address}, CurrentRawValue: {controlPointItem.CurrentRawValue}, Alarm: {controlPointItem.Alarm}, ScalingFactor: {controlPointItem.ScalingFactor}, Deviation: {controlPointItem.Deviation}, MinRawValue: {controlPointItem.MinRawValue}, MaxRawValue: {controlPointItem.MaxRawValue}, NormalValue: {controlPointItem.NormalValue}, RegisterType: {controlPointItem.RegisterType}, Name: {controlPointItem.Name}, Initialized: {controlPointItem.Initialized}";
                            Logger.LogDebug(debugMessage);
                        }
                        else
                        {
                            string warningMessage = $"{baseLogString} ImportAnalog => Control after CurrentGidToPointItemMap.SetAsync => Gid: 0x{gid:X16} was not found in reliable collection '{ReliableDictionaryNames.GidToPointItemMap}' after the value was supposedly set.";
                            Logger.LogWarning(warningMessage);
                        }
#endif

                        short registerType = (short)analogPoint.RegisterType;
                        if (!(await CurrentAddressToGidMap.ContainsKeyAsync(registerType)))
                        {
                            await CurrentAddressToGidMap.SetAsync(registerType, new Dictionary<ushort, long>());
                        }

                        var addressToGidDictionaryResult = await CurrentAddressToGidMap.TryGetValueAsync(registerType);
                        if(!addressToGidDictionaryResult.HasValue)
                        {
                            string message = $"{baseLogString} ImportAnalog => reliable collection '{ReliableDictionaryNames.AddressToGidMap}' is not initialized properly.";
                            Logger.LogError(message);
                            throw new InternalSCADAServiceException(message);
                        }
                        
                        var addressToGidDictionary = addressToGidDictionaryResult.Value;

                        if (addressToGidDictionary.ContainsKey(analogPoint.Address))
                        {
                            string message = $"{baseLogString} ImportAnalog => SCADA model is invalid => Address: {analogPoint.Address} (RegType: {registerType}) belongs to more than one entity.";
                            Logger.LogError(message);
                            throw new InternalSCADAServiceException(message);
                        }

                        addressToGidDictionary.Add(analogPoint.Address, rds[i].Id);
                        await CurrentAddressToGidMap.SetAsync(registerType, addressToGidDictionary);

                        debugMessage = $"{baseLogString} ImportAnalog => ANALOG measurement added to SCADA model [Gid: 0x{gid:X16}, Address: {analogPoint.Address}]";
                        Logger.LogDebug(debugMessage);
                    }

                    resourcesLeft = await this.nmsGdaClient.IteratorResourcesLeft(iteratorId);
                }

                await this.nmsGdaClient.IteratorClose(iteratorId);
                success = true;
            }
            catch (CommunicationObjectFaultedException e)
            {
                success = false;
                string message = $"{baseLogString} ImportAnalog => CommunicationObjectFaultedException caught.";
                Logger.LogError(message, e);

                await Task.Delay(2000);

                this.nmsGdaClient = NetworkModelGdaClient.CreateClient();
                this.scadaCommandingClient = ScadaCommandingClient.CreateClient();
                //todo: different logic on multiple rety?
            }
            catch (Exception ex)
            {
                success = false;
                string errorMessage = $"{baseLogString} ImportAnalog => failed with error: {ex.Message}";
                Trace.WriteLine(errorMessage);
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
                        if (rds[i] == null)
                        {
                            continue;
                        }
                        
                        long gid = rds[i].Id;
                        ModelCode type = modelResourceDesc.GetModelCodeFromId(gid);

                        DiscretePointItem discretePoint = new DiscretePointItem(AlarmConfigDataHelper.GetAlarmConfigData());

                        string debugMessage = $"{baseLogString} ImportDiscrete => Before Initialization => Gid: 0x{discretePoint.Gid:X16}, Address: {discretePoint.Address}, CurrentValue: {discretePoint.CurrentValue}, Alarm: {discretePoint.Alarm}, AbnormalValue: {discretePoint.AbnormalValue}, DiscreteType: {discretePoint.DiscreteType}, MinValue: {discretePoint.MinValue}, MaxValue: {discretePoint.MaxValue}, NormalValue: {discretePoint.NormalValue}, RegisterType: {discretePoint.RegisterType}, Name: {discretePoint.Name}, Initialized: {discretePoint.Initialized}";
                        Logger.LogDebug(debugMessage);

                        pointItemHelper.InitializeDiscretePointItem(discretePoint, rds[i].Properties, ModelCode.DISCRETE, enumDescs);

                        debugMessage = $"{baseLogString} ImportDiscrete => After Initialization => Gid: 0x{discretePoint.Gid:X16}, Address: {discretePoint.Address}, CurrentValue: {discretePoint.CurrentValue}, Alarm: {discretePoint.Alarm}, AbnormalValue: {discretePoint.AbnormalValue}, DiscreteType: {discretePoint.DiscreteType}, MinValue: {discretePoint.MinValue}, MaxValue: {discretePoint.MaxValue}, NormalValue: {discretePoint.NormalValue}, RegisterType: {discretePoint.RegisterType}, Name: {discretePoint.Name}, Initialized: {discretePoint.Initialized}";
                        Logger.LogDebug(debugMessage);

                        if (await CurrentGidToPointItemMap.ContainsKeyAsync(gid))
                        {
                            string errorMessage = $"{baseLogString} ImportDiscrete => SCADA model is invalid => Gid: 0x{gid:X16} belongs to more than one entity.";
                            Logger.LogError(errorMessage);
                            throw new InternalSCADAServiceException(errorMessage);
                        }

                        await CurrentGidToPointItemMap.SetAsync(gid, discretePoint);

#if DEBUG
                        var pointItemResult = await CurrentGidToPointItemMap.TryGetValueAsync(gid);
                        if (pointItemResult.HasValue)
                        {
                            DiscretePointItem controlPointItem = pointItemResult.Value as DiscretePointItem;
                            debugMessage = $"{baseLogString} ImportDiscrete => Control after CurrentGidToPointItemMap.SetAsync => Gid: 0x{controlPointItem.Gid:X16}, Address: {controlPointItem.Address}, CurrentValue: {controlPointItem.CurrentValue}, Alarm: {controlPointItem.Alarm}, AbnormalValue: {controlPointItem.AbnormalValue}, DiscreteType: {controlPointItem.DiscreteType}, MinValue: {controlPointItem.MinValue}, MaxValue: {controlPointItem.MaxValue}, NormalValue: {controlPointItem.NormalValue}, RegisterType: {controlPointItem.RegisterType}, Name: {controlPointItem.Name}, Initialized: {controlPointItem.Initialized}";
                            Logger.LogDebug(debugMessage);
                        }
                        else
                        {
                            string warningMessage = $"{baseLogString} ImportDiscrete => Control after CurrentGidToPointItemMap.SetAsync => Gid: 0x{gid:X16} was not found in reliable collection '{ReliableDictionaryNames.GidToPointItemMap}' after the value was supposedly set.";
                            Logger.LogWarning(warningMessage);
                        }
#endif
                        short registerType = (short)discretePoint.RegisterType;
                        if (!(await CurrentAddressToGidMap.ContainsKeyAsync(registerType)))
                        {
                            await CurrentAddressToGidMap.SetAsync(registerType, new Dictionary<ushort, long>());
                        }

                        var addressToGidDictionaryResult = await CurrentAddressToGidMap.TryGetValueAsync(registerType);
                        if (!addressToGidDictionaryResult.HasValue)
                        {
                            string message = $"{baseLogString} ImportDiscrete => reliable collection '{ReliableDictionaryNames.AddressToGidMap}' is not initialized properly.";
                            Logger.LogError(message);
                            throw new InternalSCADAServiceException(message);
                        }

                        var addressToGidDictionary = addressToGidDictionaryResult.Value;

                        if (addressToGidDictionary.ContainsKey(discretePoint.Address))
                        {
                            string errorMessage = $"{baseLogString} ImportDiscrete => SCADA model is invalid => Address: {discretePoint.Address} (RegType: {registerType}) belongs to more than one entity.";
                            Logger.LogError(errorMessage);
                            throw new InternalSCADAServiceException(errorMessage);
                        }

                        addressToGidDictionary.Add(discretePoint.Address, rds[i].Id);
                        await CurrentAddressToGidMap.SetAsync(registerType, addressToGidDictionary);
                        
                        debugMessage = $"{baseLogString} ImportDiscrete => ANALOG measurement added to SCADA model [Gid: 0x{gid:X16}, Address: {discretePoint.Address}]";
                        Logger.LogDebug(debugMessage);
                    }

                    resourcesLeft = await this.nmsGdaClient.IteratorResourcesLeft(iteratorId);
                }

                await this.nmsGdaClient.IteratorClose(iteratorId);
                success = true;
            }
            catch (CommunicationObjectFaultedException e)
            {
                success = false;
                string message = $"{baseLogString} ImportAnalog => CommunicationObjectFaultedException caught.";
                Logger.LogError(message, e);

                await Task.Delay(2000);

                this.nmsGdaClient = NetworkModelGdaClient.CreateClient();
                this.scadaCommandingClient = ScadaCommandingClient.CreateClient();
                //todo: different logic on multiple rety?
            }
            catch (Exception ex)
            {
                success = false;
                string errorMessage = $"{baseLogString} ImportDiscrete => failed with error: {ex.Message}";
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

                var enumerableCurrentGidToPointItemMap = await CurrentGidToPointItemMap.GetEnumerableDictionaryAsync();
                incomingGidToPointItemMap = new Dictionary<long, IScadaModelPointItem>(enumerableCurrentGidToPointItemMap.Count);

                foreach (long gid in enumerableCurrentGidToPointItemMap.Keys)
                {
                    ModelCode type = modelResourceDesc.GetModelCodeFromId(gid);
                    IScadaModelPointItem pointItem = enumerableCurrentGidToPointItemMap[gid].Clone();

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
                            string errorMessage = $"Model update data in fault state. Deleting entity with gid: {gid:X16}, that does not exists in SCADA model.";
                            Logger.LogError(errorMessage);
                            throw new ArgumentException(errorMessage);
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

        public async Task Commit()
        {
            //todo: currentScadaModel  = IncomingScadaModel;
            incomingGidToPointItemMap = null;

            //todo: currentAddressToGidMap = IncomingAddressToGidMap;
            incomingAddressToGidMap = null;

            modelChanges.Clear();
            await CommandDescriptionCache.ClearAsync();

            string message = $"Incoming SCADA model is confirmed.";
            Console.WriteLine(message);
            Logger.LogInformation(message);

            //TODO: model confirmation => modbus MU commands
            //SignalIncomingModelConfirmation.Invoke(new List<long>(CurrentScadaModel.Keys));
        }

        public Task Rollback()
        {
            return Task.Run(() =>
            {
                incomingGidToPointItemMap = null;
                incomingAddressToGidMap = null;
                modelChanges.Clear();

                string message = $"Incoming SCADA model is rejected.";
                Console.WriteLine(message);
                Logger.LogInformation(message);
            });
        }
        #endregion ITransactionActorContract

        #region Private Methods
        private async Task SendModelUpdateCommands()
        {
            var enumerableAddressToGidMapResult = await CurrentAddressToGidMap.GetEnumerableDictionaryAsync();
            var analogItemsAddressToGidMap = enumerableAddressToGidMapResult[(short)PointType.ANALOG_OUTPUT];
            var discreteItemsAddressToGidMap = enumerableAddressToGidMapResult[(short)PointType.DIGITAL_OUTPUT];

            var analogCommandingValues = new Dictionary<long, float>(analogItemsAddressToGidMap.Count);
            var discreteCommandingValues = new Dictionary<long, ushort>(discreteItemsAddressToGidMap.Count);

            foreach (long gid in analogItemsAddressToGidMap.Values)
            {
                var result = await CurrentGidToPointItemMap.TryGetValueAsync(gid);
                var analogPointItem = result.Value as IAnalogPointItem;

                analogCommandingValues.Add(gid, analogPointItem.CurrentEguValue);
            }

            foreach (long gid in discreteItemsAddressToGidMap.Values)
            {
                var result = await CurrentGidToPointItemMap.TryGetValueAsync(gid);
                var discretePointItem = result.Value as IDiscretePointItem;

                discreteCommandingValues.Add(gid, discretePointItem.CurrentValue);
            }

            await this.scadaCommandingClient.SendMultipleAnalogCommand(analogCommandingValues, CommandOriginType.MODEL_UPDATE_COMMAND);
            await this.scadaCommandingClient.SendMultipleDiscreteCommand(discreteCommandingValues, CommandOriginType.MODEL_UPDATE_COMMAND);
        }

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
                string errMessage = $"ResourceDescription type is neither analog nor digital. Type: {type}.";
                Logger.LogWarning(errMessage);
                pointItem = null;
            }

            return pointItem;
        }
        #endregion Private Methods
    }
}

using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Exceptions.SCADA;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.NmsContracts;
using OMS.Common.NmsContracts.GDA;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts.Commanding;
using OMS.Common.ScadaContracts.DataContracts.ScadaModelPointItems;
using OMS.Common.WcfClient.NMS;
using OMS.Common.WcfClient.SCADA;
using SCADA.ModelProviderImplementation.Data;
using SCADA.ModelProviderImplementation.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SCADA.ModelProviderImplementation
{
    public sealed class ScadaModelImporter
    {
        private readonly string baseLogString;
        private readonly EnumDescs enumDescs;
        private readonly ModelResourcesDesc modelResourceDesc;
        private readonly ScadaModelPointItemHelper pointItemHelper;
        private readonly IReliableStateManager stateManager;

        //private INetworkModelGDAContract nmsGdaClient;
        //private IScadaCommandingContract scadaCommandingClient;

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        #region Reliable Dictionaries
        private bool isGidToPointItemMapInitialized;
        private bool isAddressToGidMapInitialized;
        private bool isInfoCacheInitialized;

        private bool ReliableDictionariesInitialized
        {
            get 
            { 
                return isGidToPointItemMapInitialized && 
                       isAddressToGidMapInitialized && 
                       isInfoCacheInitialized;
            }
        }

        private ReliableDictionaryAccess<long, IScadaModelPointItem> GidToPointItemMap { get; set; }
        private ReliableDictionaryAccess<short, Dictionary<ushort, long>> AddressToGidMap { get; set; }
        private ReliableDictionaryAccess<string, bool> InfoCache { get; set; }

        private async void OnStateManagerChangedHandler(object sender, NotifyStateManagerChangedEventArgs e)
        {
            if (e.Action == NotifyStateManagerChangedAction.Add)
            {
                var operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
                string reliableStateName = operation.ReliableState.Name.AbsolutePath;

                if (reliableStateName == ReliableDictionaryNames.GidToPointItemMap)
                {
                    GidToPointItemMap = await ReliableDictionaryAccess<long, IScadaModelPointItem>.Create(stateManager, ReliableDictionaryNames.GidToPointItemMap);
                    this.isGidToPointItemMapInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.GidToPointItemMap}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.AddressToGidMap)
                {
                    AddressToGidMap = await ReliableDictionaryAccess<short, Dictionary<ushort, long>>.Create(stateManager, ReliableDictionaryNames.AddressToGidMap);
                    this.isAddressToGidMapInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.AddressToGidMap}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.InfoCache)
                {
                    InfoCache = await ReliableDictionaryAccess<string, bool>.Create(stateManager, ReliableDictionaryNames.InfoCache);
                    isInfoCacheInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.InfoCache}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
            }
        }
        #endregion Reliable Dictionaries

        public ScadaModelImporter(IReliableStateManager stateManager, ModelResourcesDesc modelResourceDesc, EnumDescs enumDescs)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            this.isGidToPointItemMapInitialized = false;
            this.isAddressToGidMapInitialized = false;
            this.isInfoCacheInitialized = false;

            this.stateManager = stateManager;
            this.stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;

            this.modelResourceDesc = modelResourceDesc;
            this.enumDescs = enumDescs;
            this.pointItemHelper = new ScadaModelPointItemHelper();
        }

        public async Task<bool> InitializeScadaModel()
        {
            bool success;

            string verboseMessage = $"{baseLogString} InitializeScadaModel method called.";
            Logger.LogVerbose(verboseMessage);

            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            try
            {
                success = await ImportModel();
                await InfoCache.SetAsync("IsScadaModelImported", success);

                if (success)
                {
                    await SendModelUpdateCommands();
                }
                else
                {
                    string message = $"{baseLogString} InitializeScadaModel => failed to import model";
                    Logger.LogWarning(message);
                }

            }
            catch (Exception e)
            {
                success = false;
                string errorMessage = $"{baseLogString} InitializeScadaModel => Exception caught.";
                Logger.LogError(errorMessage, e);
            }

            return success;
        }

        #region ImportScadaModel
        public async Task<bool> ImportModel(bool isRetry = false)
        {
            bool success;

            await GidToPointItemMap.ClearAsync();

            var enumerableCurrentAddressToGidMap = await AddressToGidMap.GetEnumerableDictionaryAsync();
            foreach (var key in enumerableCurrentAddressToGidMap.Keys)
            {
                var dictionary = enumerableCurrentAddressToGidMap[key];
                dictionary.Clear();

                await AddressToGidMap.SetAsync(key, dictionary);
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
                await GidToPointItemMap.ClearAsync();

                enumerableCurrentAddressToGidMap = await AddressToGidMap.GetEnumerableDictionaryAsync();
                foreach (var key in enumerableCurrentAddressToGidMap.Keys)
                {
                    var dictionary = enumerableCurrentAddressToGidMap[key];
                    dictionary.Clear();

                    await AddressToGidMap.SetAsync(key, dictionary);
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
                var nmsGdaClient = NetworkModelGdaClient.CreateClient();
                int iteratorId = await nmsGdaClient.GetExtentValues(ModelCode.ANALOG, props);
                int resourcesLeft = await nmsGdaClient.IteratorResourcesLeft(iteratorId);

                while (resourcesLeft > 0)
                {
                    List<ResourceDescription> rds = await nmsGdaClient.IteratorNext(numberOfResources, iteratorId);

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

                        if(await GidToPointItemMap.ContainsKeyAsync(gid))
                        {
                            string errorMessage = $"{baseLogString} ImportAnalog => SCADA model is invalid => Gid: 0x{gid:16} belongs to more than one entity.";
                            Logger.LogError(errorMessage);
                            throw new InternalSCADAServiceException(errorMessage);
                        }

                        await GidToPointItemMap.SetAsync(gid, analogPoint);

#if DEBUG
                        var pointItemResult = await GidToPointItemMap.TryGetValueAsync(gid);
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
                        if (!(await AddressToGidMap.ContainsKeyAsync(registerType)))
                        {
                            await AddressToGidMap.SetAsync(registerType, new Dictionary<ushort, long>());
                        }

                        var addressToGidDictionaryResult = await AddressToGidMap.TryGetValueAsync(registerType);
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
                        await AddressToGidMap.SetAsync(registerType, addressToGidDictionary);

                        debugMessage = $"{baseLogString} ImportAnalog => ANALOG measurement added to SCADA model [Gid: 0x{gid:X16}, Address: {analogPoint.Address}]";
                        Logger.LogDebug(debugMessage);
                    }

                    resourcesLeft = await nmsGdaClient.IteratorResourcesLeft(iteratorId);
                }

                await nmsGdaClient.IteratorClose(iteratorId);
                success = true;
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
                var nmsGdaClient = NetworkModelGdaClient.CreateClient();
                int iteratorId = await nmsGdaClient.GetExtentValues(ModelCode.DISCRETE, props);
                int resourcesLeft = await nmsGdaClient.IteratorResourcesLeft(iteratorId);

                while (resourcesLeft > 0)
                {
                    List<ResourceDescription> rds = await nmsGdaClient.IteratorNext(numberOfResources, iteratorId);

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

                        if (await GidToPointItemMap.ContainsKeyAsync(gid))
                        {
                            string errorMessage = $"{baseLogString} ImportDiscrete => SCADA model is invalid => Gid: 0x{gid:X16} belongs to more than one entity.";
                            Logger.LogError(errorMessage);
                            throw new InternalSCADAServiceException(errorMessage);
                        }

                        await GidToPointItemMap.SetAsync(gid, discretePoint);

#if DEBUG
                        var pointItemResult = await GidToPointItemMap.TryGetValueAsync(gid);
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
                        if (!(await AddressToGidMap.ContainsKeyAsync(registerType)))
                        {
                            await AddressToGidMap.SetAsync(registerType, new Dictionary<ushort, long>());
                        }

                        var addressToGidDictionaryResult = await AddressToGidMap.TryGetValueAsync(registerType);
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
                        await AddressToGidMap.SetAsync(registerType, addressToGidDictionary);
                        
                        debugMessage = $"{baseLogString} ImportDiscrete => ANALOG measurement added to SCADA model [Gid: 0x{gid:X16}, Address: {discretePoint.Address}]";
                        Logger.LogDebug(debugMessage);
                    }

                    resourcesLeft = await nmsGdaClient.IteratorResourcesLeft(iteratorId);
                }

                await nmsGdaClient.IteratorClose(iteratorId);
                success = true;
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

        #region Private Methods
        private async Task SendModelUpdateCommands()
        {
            var scadaCommandingClient = ScadaCommandingClient.CreateClient();
            var enumerableAddressToGidMapResult = await AddressToGidMap.GetEnumerableDictionaryAsync();

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
                        var result = await GidToPointItemMap.TryGetValueAsync(gid);
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
                        var result = await GidToPointItemMap.TryGetValueAsync(gid);
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
        #endregion Private Methods
    }
}

using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts.DataContracts;
using OMS.Common.ScadaContracts.DataContracts.ScadaModelPointItems;
using OMS.Common.ScadaContracts.ModelProvider;
using SCADA.ModelProviderImplementation.Helpers;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SCADA.ModelProviderImplementation.ContractProviders
{
    public class ModelReadAccessProvider : IScadaModelReadAccessContract
    {
        private readonly string baseLogString;
        private readonly IReliableStateManager stateManager;

        #region Reliable Dictionaries
        private bool isGidToPointItemMapInitialized;
        private bool isAddressToGidMapInitialized;
        private bool isCommandDescriptionCacheInitialized;
        private bool isInfoCacheInitialized;

        private bool ReliableDictionariesInitialized
        {
            get 
            {
                return true;
                //return isGidToPointItemMapInitialized && 
                //       isAddressToGidMapInitialized && 
                //       isCommandDescriptionCacheInitialized && 
                //       isInfoCacheInitialized;
            }
        }

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        private ReliableDictionaryAccess<long, IScadaModelPointItem> gidToPointItemMap;
        private ReliableDictionaryAccess<long, IScadaModelPointItem> GidToPointItemMap
        {
            get { return gidToPointItemMap; }
        }

        private ReliableDictionaryAccess<short, Dictionary<ushort, long>> addressToGidMap;
        private ReliableDictionaryAccess<short, Dictionary<ushort, long>> AddressToGidMap
        {
            get { return addressToGidMap; }
        }

        private ReliableDictionaryAccess<long, CommandDescription> commandDescriptionCache;
        private ReliableDictionaryAccess<long, CommandDescription> CommandDescriptionCache
        {
            get { return commandDescriptionCache; }
        }

        private ReliableDictionaryAccess<string, bool> infoCache;
        private ReliableDictionaryAccess<string, bool> InfoCache
        {
            get { return infoCache; }
        }

        private async void OnStateManagerChangedHandler(object sender, NotifyStateManagerChangedEventArgs eventArgs)
        {
            try
            {
                await InitializeReliableCollections(eventArgs);
            }
            catch (FabricNotPrimaryException)
            {
                Logger.LogDebug($"{baseLogString} OnStateManagerChangedHandler => NotPrimaryException. To be ignored.");
            }
            catch (FabricObjectClosedException)
            {
                Logger.LogDebug($"{baseLogString} OnStateManagerChangedHandler => FabricObjectClosedException. To be ignored.");
            }
            catch (COMException)
            {
                Logger.LogDebug($"{baseLogString} OnStateManagerChangedHandler => {typeof(COMException)}. To be ignored.");
            }
        }

        private async Task InitializeReliableCollections(NotifyStateManagerChangedEventArgs e)
        {
            if (e.Action == NotifyStateManagerChangedAction.Add)
            {
                var operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
                string reliableStateName = operation.ReliableState.Name.AbsolutePath;

                if (reliableStateName == ReliableDictionaryNames.GidToPointItemMap)
                {
                    gidToPointItemMap = await ReliableDictionaryAccess<long, IScadaModelPointItem>.Create(stateManager, ReliableDictionaryNames.GidToPointItemMap);
                    isGidToPointItemMapInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.GidToPointItemMap}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.AddressToGidMap)
                {
                    addressToGidMap = await ReliableDictionaryAccess<short, Dictionary<ushort, long>>.Create(stateManager, ReliableDictionaryNames.AddressToGidMap);
                    isAddressToGidMapInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.AddressToGidMap}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.CommandDescriptionCache)
                {
                    commandDescriptionCache = await ReliableDictionaryAccess<long, CommandDescription>.Create(stateManager, ReliableDictionaryNames.CommandDescriptionCache);
                    isCommandDescriptionCacheInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.CommandDescriptionCache}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.InfoCache)
                {
                    infoCache = await ReliableDictionaryAccess<string, bool>.Create(stateManager, ReliableDictionaryNames.InfoCache);
                    isInfoCacheInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.InfoCache}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
            }
        }
        #endregion Reliable Dictionaries

        public ModelReadAccessProvider(IReliableStateManager stateManager)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>{Environment.NewLine}";

            this.isGidToPointItemMapInitialized = false;
            this.isAddressToGidMapInitialized = false;
            this.isCommandDescriptionCacheInitialized = false;
            this.isInfoCacheInitialized = false;
            
            this.stateManager = stateManager;
            //this.stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;
        }

        #region IScadaModelReadAccessContract
        public async Task<bool> GetIsScadaModelImportedIndicator()
        {
            string verboseMessage = $"{baseLogString} entering GetIsScadaModelImportedIndicator method.";
            Logger.LogVerbose(verboseMessage);

            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            string key = "IsScadaModelImported";
            if (!await InfoCache.ContainsKeyAsync(key))
            {
                await InfoCache.SetAsync(key, false);
            }

            bool isScadaModelImported = (await InfoCache.TryGetValueAsync(key)).Value;
            verboseMessage = $"{baseLogString} GetIsScadaModelImportedIndicator => returning value: {isScadaModelImported}.";
            Logger.LogVerbose(verboseMessage);

            return isScadaModelImported;
        }

        public async Task<IScadaConfigData> GetScadaConfigData()
        {
            string verboseMessage = $"{baseLogString} entering GetScadaConfigData method.";
            Logger.LogVerbose(verboseMessage);

            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            verboseMessage = $"{baseLogString} GetScadaConfigData => about to execut ScadaConfigDataHelper.GetScadaConfigData().";
            Logger.LogVerbose(verboseMessage);

            var config = ScadaConfigDataHelper.GetScadaConfigData();

            verboseMessage = $"{baseLogString} GetScadaConfigData => ScadaConfigDataHelper.GetScadaConfigData() SUCCESSFULLY executed.";
            Logger.LogVerbose(verboseMessage);

            return config;
        }

        public async Task<Dictionary<long, IScadaModelPointItem>> GetGidToPointItemMap()
        {
            string verboseMessage = $"{baseLogString} entering GetGidToPointItemMap method.";
            Logger.LogVerbose(verboseMessage);

            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            verboseMessage = $"{baseLogString} GetGidToPointItemMap => about to execut GidToPointItemMap.GetDataCopy().";
            Logger.LogVerbose(verboseMessage);

            var copy = await GidToPointItemMap.GetDataCopyAsync();

            verboseMessage = $"{baseLogString} GetGidToPointItemMap => GidToPointItemMap.GetDataCopy() SUCCESSFULLY executed. Returning the collection with {copy.Count} elements.";
            Logger.LogVerbose(verboseMessage);

            return copy;
        }

        public async Task<Dictionary<short, Dictionary<ushort, IScadaModelPointItem>>> GetAddressToPointItemMap()
        {
            string verboseMessage = $"{baseLogString} entering GetAddressToPointItemMap method.";
            Logger.LogVerbose(verboseMessage);

            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            Dictionary<long, IScadaModelPointItem> gidToPointItemMap = await GetGidToPointItemMap();
            Dictionary<short, Dictionary<ushort, long>> addressToGidMap = await GetAddressToGidMap();
            Dictionary<short, Dictionary<ushort, IScadaModelPointItem>> addressToPointItemMap = new Dictionary<short, Dictionary<ushort, IScadaModelPointItem>>(addressToGidMap.Count);

            foreach (short key in addressToGidMap.Keys)
            {
                foreach (ushort address in addressToGidMap[key].Keys)
                {
                    addressToPointItemMap[key][address] = gidToPointItemMap[addressToGidMap[key][address]];
                }
            }

            verboseMessage = $"{baseLogString} GetAddressToPointItemMap => returning collection with {addressToPointItemMap.Count} elements.";
            Logger.LogVerbose(verboseMessage);

            return addressToPointItemMap;
        }

        public async Task<Dictionary<short, Dictionary<ushort, long>>> GetAddressToGidMap()
        {
            string verboseMessage = $"{baseLogString} entering GetAddressToGidMap method.";
            Logger.LogVerbose(verboseMessage);

            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            verboseMessage = $"{baseLogString} GetAddressToGidMap => about to execut AddressToGidMap.GetDataCopy().";
            Logger.LogVerbose(verboseMessage);

            var copy = await AddressToGidMap.GetDataCopyAsync();
            
            verboseMessage = $"{baseLogString} GetAddressToGidMap => AddressToGidMap.GetDataCopy() SUCCESSFULLY executed. Returning the collection with {copy.Count} elements.";
            Logger.LogVerbose(verboseMessage);

            return copy;
        }

        public async Task<Dictionary<long, CommandDescription>> GetCommandDescriptionCache()
        {
            string verboseMessage = $"{baseLogString} entering GetCommandDescriptionCache method.";
            Logger.LogVerbose(verboseMessage);

            while (!ReliableDictionariesInitialized)
            {
                await Task.Delay(1000);
            }

            verboseMessage = $"{baseLogString} GetCommandDescriptionCache => about to execut CommandDescriptionCache.GetDataCopy().";
            Logger.LogVerbose(verboseMessage);

            var copy = await CommandDescriptionCache.GetDataCopyAsync();

            if(copy.Count > 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine("GettingCommandDescriptionCache => elements:");

                foreach (var element in copy.Values)
                {
                    sb.AppendLine($"Gid: {element.Gid}, Address: {element.Address}, Value: {element.Value}, CommandOrigin: {element.CommandOrigin}");
                }

                Logger.LogDebug(sb.ToString());
            }

            verboseMessage = $"{baseLogString} GetCommandDescriptionCache => CommandDescriptionCache.GetDataCopy() SUCCESSFULLY executed. Returning the collection with {copy.Count} elements.";
            Logger.LogVerbose(verboseMessage);

            return copy;
        }

        public Task<bool> IsAlive()
        {
            return Task.Run(() => { return true; });
        }
        #endregion IScadaModelReadAccessContract
    }
}

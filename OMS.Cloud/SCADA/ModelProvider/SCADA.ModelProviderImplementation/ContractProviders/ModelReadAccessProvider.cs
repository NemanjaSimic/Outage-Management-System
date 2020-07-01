﻿using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
using OMS.Common.Cloud.Logger;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts.DataContracts;
using OMS.Common.ScadaContracts.DataContracts.ScadaModelPointItems;
using OMS.Common.ScadaContracts.ModelProvider;
using SCADA.ModelProviderImplementation.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SCADA.ModelProviderImplementation.ContractProviders
{
    public class ModelReadAccessProvider : IScadaModelReadAccessContract
    {
        private readonly string baseLogString;
        private readonly IReliableStateManager stateManager;
        
        #region Private Properties
        private bool isGidToPointItemMapInitialized;
        private bool isAddressToGidMapInitialized;
        private bool isCommandDescriptionCacheInitialized;
        private bool isInfoCacheInitialized;
        private bool ReliableDictionariesInitialized
        {
            get { return isGidToPointItemMapInitialized && isAddressToGidMapInitialized && isCommandDescriptionCacheInitialized && isInfoCacheInitialized; }
        }

        private ICloudLogger logger;
        private ICloudLogger Logger
        {
            get { return logger ?? (logger = CloudLoggerFactory.GetLogger()); }
        }

        private ReliableDictionaryAccess<long, IScadaModelPointItem> gidToPointItemMap;
        private ReliableDictionaryAccess<long, IScadaModelPointItem> GidToPointItemMap
        {
            get
            {
                return gidToPointItemMap ?? (gidToPointItemMap = ReliableDictionaryAccess<long, IScadaModelPointItem>.Create(stateManager, ReliableDictionaryNames.GidToPointItemMap).Result);
            }
        }

        private ReliableDictionaryAccess<short, Dictionary<ushort, long>> addressToGidMap;
        private ReliableDictionaryAccess<short, Dictionary<ushort, long>> AddressToGidMap
        {
            get
            {
                return addressToGidMap ?? (addressToGidMap = ReliableDictionaryAccess<short, Dictionary<ushort, long>>.Create(stateManager, ReliableDictionaryNames.AddressToGidMap).Result);
            }
        }

        private ReliableDictionaryAccess<long, CommandDescription> commandDescriptionCache;
        private ReliableDictionaryAccess<long, CommandDescription> CommandDescriptionCache
        {
            get
            {
                return commandDescriptionCache ?? (commandDescriptionCache = ReliableDictionaryAccess<long, CommandDescription>.Create(stateManager, ReliableDictionaryNames.CommandDescriptionCache).Result);
            }
        }

        private ReliableDictionaryAccess<string, bool> infoCache;
        private ReliableDictionaryAccess<string, bool> InfoCache
        {
            get
            {
                return infoCache ?? (infoCache = ReliableDictionaryAccess<string, bool>.Create(stateManager, ReliableDictionaryNames.InfoCache).Result);
            }
        }
        #endregion Properties

        public ModelReadAccessProvider(IReliableStateManager stateManager)
        {
            this.baseLogString = $"{this.GetType()} [{this.GetHashCode()}] =>";

            this.stateManager = stateManager;

            this.isGidToPointItemMapInitialized = false;
            this.isAddressToGidMapInitialized = false;
            this.isCommandDescriptionCacheInitialized = false;
            this.isInfoCacheInitialized = false;

            stateManager.StateManagerChanged += this.OnStateManagerChangedHandler;
        }

        private async void OnStateManagerChangedHandler(object sender, NotifyStateManagerChangedEventArgs e)
        {
            if (e.Action == NotifyStateManagerChangedAction.Add)
            {
                var operation = e as NotifyStateManagerSingleEntityChangedEventArgs;
                string reliableStateName = operation.ReliableState.Name.AbsolutePath;

                if (reliableStateName == ReliableDictionaryNames.GidToPointItemMap)
                {
                    //_ = GidToPointItemMap;
                    gidToPointItemMap = await ReliableDictionaryAccess<long, IScadaModelPointItem>.Create(stateManager, ReliableDictionaryNames.GidToPointItemMap);
                    isGidToPointItemMapInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.GidToPointItemMap}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.AddressToGidMap)
                {
                    //_ = AddressToGidMap;
                    addressToGidMap = await ReliableDictionaryAccess<short, Dictionary<ushort, long>>.Create(stateManager, ReliableDictionaryNames.AddressToGidMap);
                    isAddressToGidMapInitialized = true;

                    string debugMessage = $"{baseLogString} OnStateManagerChangedHandler => '{ReliableDictionaryNames.AddressToGidMap}' ReliableDictionaryAccess initialized.";
                    Logger.LogDebug(debugMessage);
                }
                else if (reliableStateName == ReliableDictionaryNames.CommandDescriptionCache)
                {
                    //_ = CommandDescriptionCache;
                    commandDescriptionCache = await ReliableDictionaryAccess<long, CommandDescription>.Create(stateManager, ReliableDictionaryNames.CommandDescriptionCache);
                    isCommandDescriptionCacheInitialized = true;

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
        }

        #region IScadaModelReadAccessContract
        public async Task<bool> GetIsScadaModelImportedIndicator()
        {
            string verboseMessage = $"{baseLogString} entering GetIsScadaModelImportedIndicator method.";
            Logger.LogVerbose(verboseMessage);

            while (!ReliableDictionariesInitialized)
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            string key = "IsScadaModelImported";
            if (!InfoCache.ContainsKey(key))
            {
                InfoCache[key] = false;
            }

            verboseMessage = $"{baseLogString} GetIsScadaModelImportedIndicator => returning value: {InfoCache[key]}.";
            Logger.LogVerbose(verboseMessage);

            return InfoCache[key];
        }

        public async Task<IScadaConfigData> GetScadaConfigData()
        {
            string verboseMessage = $"{baseLogString} entering GetScadaConfigData method.";
            Logger.LogVerbose(verboseMessage);

            while (!ReliableDictionariesInitialized || !(await GetIsScadaModelImportedIndicator()))
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            string debugMessage = $"{baseLogString} GetScadaConfigData => about to execut ScadaConfigDataHelper.GetScadaConfigData().";
            Logger.LogDebug(debugMessage);

            var config = ScadaConfigDataHelper.GetScadaConfigData();

            debugMessage = $"{baseLogString} GetScadaConfigData => ScadaConfigDataHelper.GetScadaConfigData() SUCCESSFULLY executed.";
            Logger.LogDebug(debugMessage);

            return config;
        }

        public async Task<Dictionary<long, IScadaModelPointItem>> GetGidToPointItemMap()
        {
            string verboseMessage = $"{baseLogString} entering GetGidToPointItemMap method.";
            Logger.LogVerbose(verboseMessage);

            while (!ReliableDictionariesInitialized || !(await GetIsScadaModelImportedIndicator()))
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            //Dictionary<long, IScadaModelPointItem> copy = GidToPointItemMap.GetDataCopy();
            //Dictionary<long, IScadaModelPointItem> result = new Dictionary<long, IScadaModelPointItem>(copy.Count);

            //foreach(var element in copy)
            //{
            //    result.Add(element.Key, element.Value);
            //}

            string debugMessage = $"{baseLogString} GetGidToPointItemMap => about to execut GidToPointItemMap.GetDataCopy().";
            Logger.LogDebug(debugMessage);

            var copy = GidToPointItemMap.GetDataCopy();

            debugMessage = $"{baseLogString} GetGidToPointItemMap => GidToPointItemMap.GetDataCopy() SUCCESSFULLY executed. Returning the collection with {copy.Count} elements.";
            Logger.LogDebug(debugMessage);

            return copy;
        }

        public async Task<Dictionary<short, Dictionary<ushort, IScadaModelPointItem>>> GetAddressToPointItemMap()
        {
            string verboseMessage = $"{baseLogString} entering GetAddressToPointItemMap method.";
            Logger.LogVerbose(verboseMessage);

            while (!ReliableDictionariesInitialized || !(await GetIsScadaModelImportedIndicator()))
            {
                //TODO: something smarter
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

            while (!ReliableDictionariesInitialized || !(await GetIsScadaModelImportedIndicator()))
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            string debugMessage = $"{baseLogString} GetAddressToGidMap => about to execut AddressToGidMap.GetDataCopy().";
            Logger.LogDebug(debugMessage);

            var copy = AddressToGidMap.GetDataCopy();

            debugMessage = $"{baseLogString} GetAddressToGidMap => AddressToGidMap.GetDataCopy() SUCCESSFULLY executed. Returning the collection with {copy.Count} elements.";
            Logger.LogDebug(debugMessage);

            return copy;
        }

        public async Task<Dictionary<long, CommandDescription>> GetCommandDescriptionCache()
        {
            string verboseMessage = $"{baseLogString} entering GetCommandDescriptionCache method.";
            Logger.LogVerbose(verboseMessage);

            while (!ReliableDictionariesInitialized || !(await GetIsScadaModelImportedIndicator()))
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            string debugMessage = $"{baseLogString} GetCommandDescriptionCache => about to execut CommandDescriptionCache.GetDataCopy().";
            Logger.LogDebug(debugMessage);

            var copy = CommandDescriptionCache.GetDataCopy();

            debugMessage = $"{baseLogString} GetCommandDescriptionCache => CommandDescriptionCache.GetDataCopy() SUCCESSFULLY executed. Returning the collection with {copy.Count} elements.";
            Logger.LogDebug(debugMessage);

            return copy;
        }

        #endregion IScadaModelReadAccessContract
    }
}

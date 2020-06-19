using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Notifications;
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
        private readonly IReliableStateManager stateManager;
        private bool isGidToPointItemMapInitialized;
        private bool isAddressToGidMapInitialized;
        private bool isCommandDescriptionCacheInitialized;
        private bool isInfoCacheInitialized;

        #region Private Properties
        private bool ReliableDictionariesInitialized
        {
            get { return isGidToPointItemMapInitialized && isAddressToGidMapInitialized && isCommandDescriptionCacheInitialized && isInfoCacheInitialized; }
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
                }
                else if (reliableStateName == ReliableDictionaryNames.AddressToGidMap)
                {
                    //_ = AddressToGidMap;
                    addressToGidMap = await ReliableDictionaryAccess<short, Dictionary<ushort, long>>.Create(stateManager, ReliableDictionaryNames.AddressToGidMap);
                    isAddressToGidMapInitialized = true;
                }
                else if (reliableStateName == ReliableDictionaryNames.CommandDescriptionCache)
                {
                    //_ = CommandDescriptionCache;
                    commandDescriptionCache = await ReliableDictionaryAccess<long, CommandDescription>.Create(stateManager, ReliableDictionaryNames.CommandDescriptionCache);
                    isCommandDescriptionCacheInitialized = true;
                }
                else if (reliableStateName == ReliableDictionaryNames.InfoCache)
                {
                    //_ = InfoCache;
                    infoCache = await ReliableDictionaryAccess<string, bool>.Create(stateManager, ReliableDictionaryNames.InfoCache);
                    isInfoCacheInitialized = true;
                }
            }
        }

        #region IScadaModelReadAccessContract
        public async Task<bool> GetIsScadaModelImportedIndicator()
        {
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

            return InfoCache[key];
        }

        public async Task<IScadaConfigData> GetScadaConfigData()
        {
            while (!ReliableDictionariesInitialized || !(await GetIsScadaModelImportedIndicator()))
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            return ScadaConfigDataHelper.GetScadaConfigData();
        }

        public async Task<Dictionary<long, IScadaModelPointItem>> GetGidToPointItemMap()
        {
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

            return GidToPointItemMap.GetDataCopy();
        }

        public async Task<Dictionary<short, Dictionary<ushort, IScadaModelPointItem>>> GetAddressToPointItemMap()
        {
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

            return addressToPointItemMap;
        }

        public async Task<Dictionary<short, Dictionary<ushort, long>>> GetAddressToGidMap()
        {
            while (!ReliableDictionariesInitialized || !(await GetIsScadaModelImportedIndicator()))
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            return AddressToGidMap.GetDataCopy();
        }

        public async Task<Dictionary<long, CommandDescription>> GetCommandDescriptionCache()
        {
            while (!ReliableDictionariesInitialized || !(await GetIsScadaModelImportedIndicator()))
            {
                //TODO: something smarter
                await Task.Delay(1000);
            }

            return CommandDescriptionCache.GetDataCopy();
        }

        #endregion IScadaModelReadAccessContract
    }
}

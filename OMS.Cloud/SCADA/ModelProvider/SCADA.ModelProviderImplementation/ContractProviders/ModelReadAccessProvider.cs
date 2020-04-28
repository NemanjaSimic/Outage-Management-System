using Common.SCADA;
using Microsoft.ServiceFabric.Data;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.ScadaContracts;
using OMS.Common.ScadaContracts.DataContracts;
using SCADA.ModelProviderImplementation.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SCADA.ModelProviderImplementation.ContractProviders
{
    public class ModelReadAccessProvider : IScadaModelReadAccessContract
    {
        private readonly IReliableStateManager stateManager;

        #region Private Properties
        private ScadaConfigData configData;
        private ScadaConfigData ConfigData
        {
            get
            {
                return configData ?? (configData = new ScadaConfigData());
            }
        }

        private ReliableDictionaryAccess<long, IScadaModelPointItem> gidToPointItemMap;
        private ReliableDictionaryAccess<long, IScadaModelPointItem> GidToPointItemMap
        {
            get
            {
                return gidToPointItemMap ?? (gidToPointItemMap = new ReliableDictionaryAccess<long, IScadaModelPointItem>(stateManager, ReliableDictionaryNames.GidToPointItemMap));
            }
        }

        private ReliableDictionaryAccess<ushort, Dictionary<ushort, long>> addressToGidMap;
        private ReliableDictionaryAccess<ushort, Dictionary<ushort, long>> AddressToGidMap
        {
            get
            {
                return addressToGidMap ?? (addressToGidMap = new ReliableDictionaryAccess<ushort, Dictionary<ushort, long>>(stateManager, ReliableDictionaryNames.AddressToGidMap));
            }
        }

        private ReliableDictionaryAccess<long, CommandDescription> commandDescriptionCache;
        private ReliableDictionaryAccess<long, CommandDescription> CommandDescriptionCache
        {
            get
            {
                return commandDescriptionCache ?? (commandDescriptionCache = new ReliableDictionaryAccess<long, CommandDescription>(stateManager, ReliableDictionaryNames.CommandDescriptionCache));
            }
        }
        #endregion Properties

        public ModelReadAccessProvider(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
            ScadaConfigDataHelper.ImportAppSettings(ConfigData);
        }

        public async Task<bool> GetIsScadaModelImportedIndicator()
        {
            throw new NotImplementedException();
        }

        public async Task<ScadaConfigData> GetScadaConfigData()
        {
            return ConfigData;
        }

        public async Task<Dictionary<long, IScadaModelPointItem>> GetGidToPointItemMap()
        {
            return GidToPointItemMap.GetDataCopy();
        }

        public async Task<Dictionary<ushort, Dictionary<ushort, IScadaModelPointItem>>> GetAddressToPointItemMap()
        {
            Dictionary<long, IScadaModelPointItem> gidToPointItemMap = await GetGidToPointItemMap();
            Dictionary<ushort, Dictionary<ushort, long>> addressToGidMap = await GetAddressToGidMap();
            Dictionary<ushort, Dictionary<ushort, IScadaModelPointItem>> addressToPointItemMap = new Dictionary<ushort, Dictionary<ushort, IScadaModelPointItem>>(addressToGidMap.Count);

            foreach (ushort key in addressToGidMap.Keys)
            {
                foreach (ushort address in addressToGidMap[key].Keys)
                {
                    addressToPointItemMap[key][address] = gidToPointItemMap[addressToGidMap[key][address]];
                }
            }

            return addressToPointItemMap;
        }

        public async Task<Dictionary<ushort, Dictionary<ushort, long>>> GetAddressToGidMap()
        {
            return AddressToGidMap.GetDataCopy();
        }

        public async Task<Dictionary<long, CommandDescription>> GetCommandDescriptionCache()
        {
            return CommandDescriptionCache.GetDataCopy();
        }
    }
}

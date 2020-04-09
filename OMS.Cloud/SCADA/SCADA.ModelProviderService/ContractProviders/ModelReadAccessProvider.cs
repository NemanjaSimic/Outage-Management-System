using Common.SCADA;
using Microsoft.ServiceFabric.Data;
using OMS.Cloud.SCADA.ModelProviderService.Configuration;
using OMS.Common.Cloud.ReliableCollectionHelpers;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.Cloud.SCADA.ModelProviderService.ContractProviders
{
    internal class ModelReadAccessProvider : IScadaModelReadAccessContract
    {
        private readonly IReliableStateManager stateManager;

        #region Private Properties
        private ISCADAConfigData configData;
        private ISCADAConfigData ConfigData
        {
            get
            {
                return configData ?? (configData = SCADAConfigData.Instance);
            }
        }

        private ReliableDictionaryAccess<long, ISCADAModelPointItem> gidToPointItemMap;
        private ReliableDictionaryAccess<long, ISCADAModelPointItem> GidToPointItemMap
        {
            get
            {
                return gidToPointItemMap ?? (gidToPointItemMap = new ReliableDictionaryAccess<long, ISCADAModelPointItem>(stateManager, ReliableDictionaryNames.GidToPointItemMap));
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
        }

        public async Task<bool> GetIsScadaModelImportedIndicator()
        {
            throw new NotImplementedException();
        }

        public async Task<ISCADAConfigData> GetScadaConfigData()
        {
            return this.configData;
        }

        public async Task<Dictionary<long, ISCADAModelPointItem>> GetGidToPointItemMap()
        {
            return GidToPointItemMap.GetDataCopy();
        }

        public async Task<Dictionary<ushort, Dictionary<ushort, ISCADAModelPointItem>>> GetAddressToPointItemMap()
        {
            Dictionary<long, ISCADAModelPointItem> gidToPointItemMap = await GetGidToPointItemMap();
            Dictionary<ushort, Dictionary<ushort, long>> addressToGidMap = await GetAddressToGidMap();
            Dictionary<ushort, Dictionary<ushort, ISCADAModelPointItem>> addressToPointItemMap = new Dictionary<ushort, Dictionary<ushort, ISCADAModelPointItem>>(addressToGidMap.Count);

            foreach(ushort key in addressToGidMap.Keys)
            {
                foreach(ushort address in addressToGidMap[key].Keys)
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

using Common.SCADA;
using Microsoft.ServiceFabric.Data;
using OMS.Cloud.SCADA.Data.Configuration;
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
        private ISCADAConfigData configData;
        private readonly ReliableDictionaryAccess<long, ISCADAModelPointItem> gidToPointItemMap;
        private readonly ReliableDictionaryAccess<ushort, Dictionary<ushort, long>> addressToGidMap;
        private readonly ReliableDictionaryAccess<long, CommandDescription> commandDescriptionCache;

        public ModelReadAccessProvider(IReliableStateManager stateManager)
        {
            this.configData = SCADAConfigData.Instance;

            this.gidToPointItemMap = new ReliableDictionaryAccess<long, ISCADAModelPointItem>(stateManager, ReliableDictionaryNames.GidToPointItemMap);
            this.addressToGidMap = new ReliableDictionaryAccess<ushort, Dictionary<ushort, long>>(stateManager, ReliableDictionaryNames.AddressToGidMap);
            this.commandDescriptionCache = new ReliableDictionaryAccess<long, CommandDescription>(stateManager, ReliableDictionaryNames.CommandDescriptionCache);
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
            return this.gidToPointItemMap.GetDataCopy();
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
            return this.addressToGidMap.GetDataCopy();
        }

        public async Task<Dictionary<long, CommandDescription>> GetCommandDescriptionCache()
        {
            return this.commandDescriptionCache.GetDataCopy();
        }        
    }
}

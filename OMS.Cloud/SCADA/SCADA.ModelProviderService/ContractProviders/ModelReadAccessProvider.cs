using OMS.Common.SCADA;
using OMS.Common.ScadaContracts;
using System;
using System.Collections.Generic;

namespace OMS.Cloud.SCADA.ModelProviderService.ContractProviders
{
    internal class ModelReadAccessProvider : IScadaModelReadAccessContract
    {
        public Dictionary<PointType, Dictionary<ushort, long>> GetAddressToGidMap()
        {
            throw new NotImplementedException();
        }

        public Dictionary<PointType, Dictionary<ushort, ISCADAModelPointItem>> GetAddressToPointItemMap()
        {
            throw new NotImplementedException();
        }

        public Dictionary<long, CommandDescription> GetCommandDescriptionCache()
        {
            throw new NotImplementedException();
        }

        public Dictionary<long, ISCADAModelPointItem> GetGidToPointItemMap()
        {
            throw new NotImplementedException();
        }

        public bool GetIsScadaModelImportedIndicator()
        {
            throw new NotImplementedException();
        }

        public ISCADAConfigData GetScadaConfigData()
        {
            throw new NotImplementedException();
        }
    }
}

using OMS.Common.SCADA;
using OMS.Common.ScadaContracts;
using System;
using System.Collections.Generic;

namespace OMS.Cloud.SCADA.ModelProviderService.ContractProviders
{
    internal class ModelAccessProvider : IScadaModelAccessContract
    {
        public Dictionary<PointType, Dictionary<ushort, long>> GetAddressToGidMapping()
        {
            throw new NotImplementedException();
        }

        public ISCADAConfigData GetScadaConfigData()
        {
            throw new NotImplementedException();
        }
    }
}

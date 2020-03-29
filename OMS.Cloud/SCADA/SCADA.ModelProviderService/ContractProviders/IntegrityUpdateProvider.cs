using OMS.Common.ScadaContracts;
using Outage.Common;
using Outage.Common.PubSub.SCADADataContract;
using System.Collections.Generic;

namespace OMS.Cloud.SCADA.ModelProviderService.ContractProviders
{
    internal class IntegrityUpdateProvider : IScadaIntegrityUpdateContract
    {
        public Dictionary<Topic, SCADAPublication> GetIntegrityUpdate()
        {
            throw new System.NotImplementedException();
        }

        public SCADAPublication GetIntegrityUpdateForSpecificTopic(Topic topic)
        {
            throw new System.NotImplementedException();
        }
    }
}

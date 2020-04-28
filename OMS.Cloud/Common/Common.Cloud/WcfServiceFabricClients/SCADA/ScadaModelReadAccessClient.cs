using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.ScadaContracts;
using OMS.Common.ScadaContracts.DataContracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.Common.Cloud.WcfServiceFabricClients.SCADA
{
    public class ScadaModelReadAccessClient : WcfSeviceFabricClientBase<IScadaModelReadAccessContract>, IScadaModelReadAccessContract
    {
        public ScadaModelReadAccessClient(WcfCommunicationClientFactory<IScadaModelReadAccessContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
            : base(clientFactory, serviceUri, servicePartition)
        {
        }

        public static ScadaModelReadAccessClient CreateClient(Uri serviceUri = null)
        {
            ClientFactory factory = new ClientFactory();
            ServicePartitionKey servicePartition = new ServicePartitionKey(0);

            if (serviceUri == null)
            {
                return factory.CreateClient<ScadaModelReadAccessClient, IScadaModelReadAccessContract>(MicroserviceNames.ScadaModelProviderService, servicePartition);
            }
            else
            {
                return factory.CreateClient<ScadaModelReadAccessClient, IScadaModelReadAccessContract>(serviceUri, servicePartition);
            }
        }

        #region IScadaModelAccessContract
        public Task<Dictionary<ushort, Dictionary<ushort, long>>> GetAddressToGidMap()
        {
            throw new NotImplementedException();
            // return InvokeWithRetryAsync(client => client.Channel.GetAddressToGidMap());
        }

        public Task<Dictionary<ushort, Dictionary<ushort, IScadaModelPointItem>>> GetAddressToPointItemMap()
        {
            throw new NotImplementedException();
            //return InvokeWithRetryAsync(client => client.Channel.GetAddressToPointItemMap());
        }

        public Task<Dictionary<long, CommandDescription>> GetCommandDescriptionCache()
        {
            throw new NotImplementedException();
            //return InvokeWithRetryAsync(client => client.Channel.GetCommandDescriptionCache());
        }

        public Task<Dictionary<long, IScadaModelPointItem>> GetGidToPointItemMap()
        {
            throw new NotImplementedException();
            //return InvokeWithRetryAsync(client => client.Channel.GetGidToPointItemMap());
        }

        public Task<bool> GetIsScadaModelImportedIndicator()
        {
            throw new NotImplementedException();
            ///return InvokeWithRetryAsync(client => client.Channel.GetIsScadaModelImportedIndicator());
        }

        public Task<ScadaConfigData> GetScadaConfigData()
        {
            return InvokeWithRetryAsync(client => client.Channel.GetScadaConfigData());
        }
        #endregion
    }
}

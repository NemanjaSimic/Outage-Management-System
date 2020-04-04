using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Fabric;
using System.Threading.Tasks;

namespace OMS.Common.Cloud.WcfServiceFabricClients.SCADA
{
    public class ScadaModelReadAccessClient : WcfSeviceFabricClientBase<IScadaModelReadAccessContract>, IScadaModelReadAccessContract
    {
        public ScadaModelReadAccessClient(WcfCommunicationClientFactory<IScadaModelReadAccessContract> clientFactory, Uri serviceUri)
            : base(clientFactory, serviceUri)
        {
        }

        public static ScadaModelReadAccessClient CreateClient(Uri serviceUri = null)
        {
            if (serviceUri == null && ConfigurationManager.AppSettings[MicroserviceNames.ScadaModelProviderService] is string scadaModelProviderServiceName)
            {
                serviceUri = new Uri(scadaModelProviderServiceName);
            }

            var partitionResolver = new ServicePartitionResolver(() => new FabricClient());
            //var partitionResolver = ServicePartitionResolver.GetDefault();
            var factory = new WcfCommunicationClientFactory<IScadaModelReadAccessContract>(TcpBindingHelper.CreateClientBinding(), null, partitionResolver);

            return new ScadaModelReadAccessClient(factory, serviceUri);
        }

        #region IScadaModelAccessContract
        public Task<Dictionary<ushort, Dictionary<ushort, long>>> GetAddressToGidMap()
        {
            return InvokeWithRetryAsync(client => client.Channel.GetAddressToGidMap());
        }

        public Task<Dictionary<ushort, Dictionary<ushort, ISCADAModelPointItem>>> GetAddressToPointItemMap()
        {
            return InvokeWithRetryAsync(client => client.Channel.GetAddressToPointItemMap());
        }

        public Task<Dictionary<long, CommandDescription>> GetCommandDescriptionCache()
        {
            return InvokeWithRetryAsync(client => client.Channel.GetCommandDescriptionCache());
        }

        public Task<Dictionary<long, ISCADAModelPointItem>> GetGidToPointItemMap()
        {
            return InvokeWithRetryAsync(client => client.Channel.GetGidToPointItemMap());
        }

        public Task<bool> GetIsScadaModelImportedIndicator()
        {
            return InvokeWithRetryAsync(client => client.Channel.GetIsScadaModelImportedIndicator());
        }

        public Task<ISCADAConfigData> GetScadaConfigData()
        {
            return InvokeWithRetryAsync(client => client.Channel.GetScadaConfigData());
        }
        #endregion
    }
}

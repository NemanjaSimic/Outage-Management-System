using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.SCADA;
using OMS.Common.ScadaContracts;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Fabric;

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
        #endregion
    }
}

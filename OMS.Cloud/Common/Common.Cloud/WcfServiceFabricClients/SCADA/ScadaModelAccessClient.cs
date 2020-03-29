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
    public class ScadaModelAccessClient : WcfSeviceFabricClientBase<IScadaModelAccessContract>, IScadaModelAccessContract
    {
        public ScadaModelAccessClient(WcfCommunicationClientFactory<IScadaModelAccessContract> clientFactory, Uri serviceName)
            : base(clientFactory, serviceName)
        {
        }

        public static ScadaModelAccessClient CreateClient(Uri nmsServiceName = null)
        {
            if (nmsServiceName == null && ConfigurationManager.AppSettings[MicroserviceNames.NmsGdaService] is string nmsGdaServiceName)
            {
                nmsServiceName = new Uri(nmsGdaServiceName);
            }

            var partitionResolver = new ServicePartitionResolver(() => new FabricClient());
            //var partitionResolver = ServicePartitionResolver.GetDefault();
            var factory = new WcfCommunicationClientFactory<IScadaModelAccessContract>(TcpBindingHelper.CreateClientBinding(), null, partitionResolver);

            return new ScadaModelAccessClient(factory, nmsServiceName);
        }

        #region IScadaModelAccessContract
        public Dictionary<PointType, Dictionary<ushort, long>> GetAddressToGidMapping()
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

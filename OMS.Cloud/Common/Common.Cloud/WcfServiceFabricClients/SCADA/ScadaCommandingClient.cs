using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.ScadaContracts;
using Outage.Common;
using System;
using System.Configuration;
using System.Fabric;

namespace OMS.Common.Cloud.WcfServiceFabricClients.SCADA
{

    public class ScadaCommandingClient : WcfSeviceFabricClientBase<IScadaCommandingContract>, IScadaCommandingContract
    {
        public ScadaCommandingClient(WcfCommunicationClientFactory<IScadaCommandingContract> clientFactory, Uri serviceName)
            : base(clientFactory, serviceName)
        {
        }

        public static ScadaCommandingClient CreateClient(Uri nmsServiceName = null)
        {
            if (nmsServiceName == null && ConfigurationManager.AppSettings[MicroserviceNames.ScadaCommandingService] is string nmsGdaServiceName)
            {
                nmsServiceName = new Uri(nmsGdaServiceName);
            }

            var partitionResolver = new ServicePartitionResolver(() => new FabricClient());
            //var partitionResolver = ServicePartitionResolver.GetDefault();
            var factory = new WcfCommunicationClientFactory<IScadaCommandingContract>(TcpBindingHelper.CreateClientBinding(), null, partitionResolver);

            return new ScadaCommandingClient(factory, nmsServiceName);
        }

        #region IScadaCommandingContract
        public bool SendAnalogCommand(long gid, float commandingValue, CommandOriginType commandOriginType)
        {
            throw new System.NotImplementedException();
        }

        public bool SendDiscreteCommand(long gid, ushort commandingValue, CommandOriginType commandOriginType)
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
}

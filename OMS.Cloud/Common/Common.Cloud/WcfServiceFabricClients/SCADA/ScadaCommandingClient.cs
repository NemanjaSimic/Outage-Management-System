using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.ScadaContracts;
using Outage.Common;
using System;
using System.Configuration;
using System.Fabric;
using System.ServiceModel;
using System.Threading.Tasks;

namespace OMS.Common.Cloud.WcfServiceFabricClients.SCADA
{

    public class ScadaCommandingClient : WcfSeviceFabricClientBase<IScadaCommandingContract>, IScadaCommandingContract
    {
        public ScadaCommandingClient(WcfCommunicationClientFactory<IScadaCommandingContract> clientFactory, Uri serviceUri)
            : base(clientFactory, serviceUri)
        {
        }

        public static ScadaCommandingClient CreateClient(Uri serviceUri = null)
        {
            if (serviceUri == null && ConfigurationManager.AppSettings[MicroserviceNames.ScadaCommandingService] is string scadaCommandingServiceName)
            {
                serviceUri = new Uri(scadaCommandingServiceName);
            }

            var partitionResolver = new ServicePartitionResolver(() => new FabricClient());
            //var partitionResolver = ServicePartitionResolver.GetDefault();
            var factory = new WcfCommunicationClientFactory<IScadaCommandingContract>(TcpBindingHelper.CreateClientBinding(), null, partitionResolver);

            return new ScadaCommandingClient(factory, serviceUri);
        }

        #region IScadaCommandingContract
        public Task<bool> SendAnalogCommand(long gid, float commandingValue, CommandOriginType commandOriginType)
        {
            return InvokeWithRetryAsync(client => client.Channel.SendAnalogCommand(gid, commandingValue, commandOriginType));
        }

        public Task<bool> SendDiscreteCommand(long gid, ushort commandingValue, CommandOriginType commandOriginType)
        {
            return InvokeWithRetryAsync(client => client.Channel.SendDiscreteCommand(gid, commandingValue, commandOriginType));
        }
        #endregion
    }
}

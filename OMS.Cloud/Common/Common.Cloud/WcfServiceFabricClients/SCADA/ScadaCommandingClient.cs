using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.ScadaContracts;
using Outage.Common;
using System;
using System.Threading.Tasks;

namespace OMS.Common.Cloud.WcfServiceFabricClients.SCADA
{

    public class ScadaCommandingClient : WcfSeviceFabricClientBase<IScadaCommandingContract>, IScadaCommandingContract
    {
        public ScadaCommandingClient(WcfCommunicationClientFactory<IScadaCommandingContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
            : base(clientFactory, serviceUri, servicePartition)
        {
        }

        public static ScadaCommandingClient CreateClient(Uri serviceUri = null)
        {
            ClientFactory factory = new ClientFactory();

            if (serviceUri == null)
            {
                return factory.CreateClient<ScadaCommandingClient, IScadaCommandingContract>(MicroserviceNames.ScadaCommandingService);
            }
            else
            {
                return factory.CreateClient<ScadaCommandingClient, IScadaCommandingContract>(serviceUri);
            }
        }

        #region IScadaCommandingContract
        public Task SendAnalogCommand(long gid, float commandingValue, CommandOriginType commandOriginType)
        {
            return InvokeWithRetryAsync(client => client.Channel.SendAnalogCommand(gid, commandingValue, commandOriginType));
        }

        public Task SendDiscreteCommand(long gid, ushort commandingValue, CommandOriginType commandOriginType)
        {
            return InvokeWithRetryAsync(client => client.Channel.SendDiscreteCommand(gid, commandingValue, commandOriginType));
        }
        #endregion
    }
}

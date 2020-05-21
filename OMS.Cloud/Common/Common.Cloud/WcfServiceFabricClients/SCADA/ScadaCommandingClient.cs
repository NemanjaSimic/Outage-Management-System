using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.ScadaContracts.Commanding;
using Outage.Common;
using System;
using System.Collections.Generic;
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
            ServicePartitionKey servicePartition = ServicePartitionKey.Singleton;

            if (serviceUri == null)
            {
                return factory.CreateClient<ScadaCommandingClient, IScadaCommandingContract>(MicroserviceNames.ScadaCommandingService, servicePartition);
            }
            else
            {
                return factory.CreateClient<ScadaCommandingClient, IScadaCommandingContract>(serviceUri, servicePartition);
            }
        }

        #region IScadaCommandingContract
        public Task SendSingleAnalogCommand(long gid, float commandingValue, CommandOriginType commandOriginType)
        {
            return InvokeWithRetryAsync(client => client.Channel.SendSingleAnalogCommand(gid, commandingValue, commandOriginType));
        }

        public Task SendMultipleAnalogCommand(Dictionary<long, float> commandingValues, CommandOriginType commandOriginType)
        {
            return InvokeWithRetryAsync(client => client.Channel.SendMultipleAnalogCommand(commandingValues, commandOriginType));
        }

        public Task SendSingleDiscreteCommand(long gid, ushort commandingValue, CommandOriginType commandOriginType)
        {
            return InvokeWithRetryAsync(client => client.Channel.SendSingleDiscreteCommand(gid, commandingValue, commandOriginType));
        }

        public Task SendMultipleDiscreteCommand(Dictionary<long, ushort> commandingValues, CommandOriginType commandOriginType)
        {
            return InvokeWithRetryAsync(client => client.Channel.SendMultipleDiscreteCommand(commandingValues, commandOriginType));
        }
        #endregion
    }
}

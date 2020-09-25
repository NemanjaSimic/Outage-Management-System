using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Names;
using OMS.Common.ScadaContracts.Commanding;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.SCADA
{

    public class ScadaCommandingClient : WcfSeviceFabricClientBase<IScadaCommandingContract>, IScadaCommandingContract
    {
        private static readonly string microserviceName = MicroserviceNames.ScadaCommandingService;
        private static readonly string listenerName = EndpointNames.ScadaCommandingEndpoint;

        public ScadaCommandingClient(WcfCommunicationClientFactory<IScadaCommandingContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
            : base(clientFactory, serviceUri, servicePartition, listenerName)
        {
        }

        public static IScadaCommandingContract CreateClient()
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<ScadaCommandingClient, IScadaCommandingContract>(microserviceName);
        }

        public static IScadaCommandingContract CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<ScadaCommandingClient, IScadaCommandingContract>(serviceUri, servicePartitionKey);
        }

        #region IScadaCommandingContract
        public Task<bool> SendSingleAnalogCommand(long gid, float commandingValue, CommandOriginType commandOriginType)
        {
            return InvokeWithRetryAsync(client => client.Channel.SendSingleAnalogCommand(gid, commandingValue, commandOriginType));
        }

        public Task<bool> SendMultipleAnalogCommand(Dictionary<long, float> commandingValues, CommandOriginType commandOriginType)
        {
            return InvokeWithRetryAsync(client => client.Channel.SendMultipleAnalogCommand(commandingValues, commandOriginType));
        }

        public Task<bool> SendSingleDiscreteCommand(long gid, ushort commandingValue, CommandOriginType commandOriginType)
        {
            return InvokeWithRetryAsync(client => client.Channel.SendSingleDiscreteCommand(gid, commandingValue, commandOriginType));
        }

        public Task<bool> SendMultipleDiscreteCommand(Dictionary<long, ushort> commandingValues, CommandOriginType commandOriginType)
        {
            return InvokeWithRetryAsync(client => client.Channel.SendMultipleDiscreteCommand(commandingValues, commandOriginType));
        }
        #endregion

        public Task<bool> IsAlive()
        {
            return InvokeWithRetryAsync(client => client.Channel.IsAlive());
        }
    }
}

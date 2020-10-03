using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Names;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using OMS.Common.ScadaContracts.DataContracts;
using OMS.Common.ScadaContracts.DataContracts.ScadaModelPointItems;
using OMS.Common.ScadaContracts.ModelProvider;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.SCADA
{
    public class ScadaModelUpdateAccessClient : WcfSeviceFabricClientBase<IScadaModelUpdateAccessContract>, IScadaModelUpdateAccessContract
    {
        private static readonly string microserviceName = MicroserviceNames.ScadaModelProviderService;
        private static readonly string listenerName = EndpointNames.ScadaModelUpdateAccessEndpoint;

        public ScadaModelUpdateAccessClient(WcfCommunicationClientFactory<IScadaModelUpdateAccessContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
            : base(clientFactory, serviceUri, servicePartition, listenerName)
        {
        }

        public static IScadaModelUpdateAccessContract CreateClient()
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<ScadaModelUpdateAccessClient, IScadaModelUpdateAccessContract>(microserviceName);
        }

        public static IScadaModelUpdateAccessContract CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<ScadaModelUpdateAccessClient, IScadaModelUpdateAccessContract>(serviceUri, servicePartitionKey);
        }

        #region IScadaModelUpdateAccessContract
        public Task MakeAnalogEntryToMeasurementCache(Dictionary<long, AnalogModbusData> data, bool permissionToPublishData)
        {
            return InvokeWithRetryAsync(client => client.Channel.MakeAnalogEntryToMeasurementCache(data, permissionToPublishData));
        }

        public Task MakeDiscreteEntryToMeasurementCache(Dictionary<long, DiscreteModbusData> data, bool permissionToPublishData)
        {
            return InvokeWithRetryAsync(client => client.Channel.MakeDiscreteEntryToMeasurementCache(data, permissionToPublishData));
        }

        public Task<ScadaModelPointItem> UpdatePointItemRawValue(long gid, int rawValue)
        {
            return InvokeWithRetryAsync(client => client.Channel.UpdatePointItemRawValue(gid, rawValue));
        }

        public Task AddOrUpdateCommandDescription(long gid, CommandDescription commandDescription)
        {
            return InvokeWithRetryAsync(client => client.Channel.AddOrUpdateCommandDescription(gid, commandDescription));
        }

        public Task AddOrUpdateMultipleCommandDescriptions(Dictionary<long, CommandDescription> commandDescriptions)
        {
            return InvokeWithRetryAsync(client => client.Channel.AddOrUpdateMultipleCommandDescriptions(commandDescriptions));
        }

        public Task<bool> RemoveCommandDescription(long gid)
        { 
            return InvokeWithRetryAsync(client => client.Channel.RemoveCommandDescription(gid));
        }

        public Task<bool> IsAlive()
        {
            return Task.Run(() => { return true; });
        }
        #endregion
    }
}

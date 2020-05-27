using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.ScadaContracts.DataContracts;
using OMS.Common.ScadaContracts.ModelProvider;
using Outage.Common;
using Outage.Common.PubSub.SCADADataContract;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.Common.Cloud.WcfServiceFabricClients.SCADA
{
    public class ScadaModelUpdateAccessClient : WcfSeviceFabricClientBase<IScadaModelUpdateAccessContract>, IScadaModelUpdateAccessContract
    {
        private static readonly string microserviceName = MicroserviceNames.ScadaModelProviderService;
        private static readonly string listenerName = EndpointNames.ScadaModelUpdateAccessEndpoint;

        public ScadaModelUpdateAccessClient(WcfCommunicationClientFactory<IScadaModelUpdateAccessContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
            : base(clientFactory, serviceUri, servicePartition, listenerName)
        {
        }

        public static ScadaModelUpdateAccessClient CreateClient(Uri serviceUri = null)
        {
            ClientFactory factory = new ClientFactory();
            ServicePartitionKey servicePartition = new ServicePartitionKey(0);

            if (serviceUri == null)
            {
                return factory.CreateClient<ScadaModelUpdateAccessClient, IScadaModelUpdateAccessContract>(microserviceName, servicePartition);
            }
            else
            {
                return factory.CreateClient<ScadaModelUpdateAccessClient, IScadaModelUpdateAccessContract>(serviceUri, servicePartition);
            }
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

        public Task UpdateCommandDescription(long gid, CommandDescription commandDescription)
        {
            return InvokeWithRetryAsync(client => client.Channel.UpdateCommandDescription(gid, commandDescription));
        }

        //public Task MakeAnalogEntryToMeasurementCache()
        //{
        //    return InvokeWithRetryAsync(client => client.Channel.MakeAnalogEntryToMeasurementCache());
        //}

        //public Task MakeDiscreteEntryToMeasurementCache()
        //{
        //    return InvokeWithRetryAsync(client => client.Channel.MakeDiscreteEntryToMeasurementCache());
        //}

        //public Task UpdateCommandDescription()
        //{
        //    return InvokeWithRetryAsync(client => client.Channel.UpdateCommandDescription());
        //}
        #endregion
    }
}

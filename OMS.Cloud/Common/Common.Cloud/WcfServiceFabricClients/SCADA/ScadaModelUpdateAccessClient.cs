using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.ScadaContracts;
using Outage.Common.PubSub.SCADADataContract;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Fabric;
using System.Threading.Tasks;

namespace OMS.Common.Cloud.WcfServiceFabricClients.SCADA
{
    public class ScadaModelUpdateAccessClient : WcfSeviceFabricClientBase<IScadaModelUpdateAccessContract>, IScadaModelUpdateAccessContract
    {
        public ScadaModelUpdateAccessClient(WcfCommunicationClientFactory<IScadaModelUpdateAccessContract> clientFactory, Uri serviceUri)
            : base(clientFactory, serviceUri)
        {
        }

        public static ScadaModelUpdateAccessClient CreateClient(Uri serviceUri = null)
        {
            if (serviceUri == null && ConfigurationManager.AppSettings[MicroserviceNames.ScadaModelProviderService] is string scadaModelProviderServiceName)
            {
                serviceUri = new Uri(scadaModelProviderServiceName);
            }

            var partitionResolver = new ServicePartitionResolver(() => new FabricClient());
            //var partitionResolver = ServicePartitionResolver.GetDefault();
            var factory = new WcfCommunicationClientFactory<IScadaModelUpdateAccessContract>(TcpBindingHelper.CreateClientBinding(), null, partitionResolver);

            return new ScadaModelUpdateAccessClient(factory, serviceUri);
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
        #endregion
    }
}

using Common.Contracts.WebAdapterContracts;
using Common.Web.Models.ViewModels;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using OMS.Common.PubSubContracts.DataContracts.SCADA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.WebAdapter
{
    public class WebAdapterClient: WcfSeviceFabricClientBase<IWebAdapterContract>, IWebAdapterContract
    {
        private static readonly string listenerName = EndpointNames.WebAdapterEndpoint;

        public WebAdapterClient(WcfCommunicationClientFactory<IWebAdapterContract> clientFactory, Uri serviceName, ServicePartitionKey servicePartition)
            : base(clientFactory, serviceName, servicePartition, listenerName)
        {
        }

        public static WebAdapterClient CreateClient(string serviceName)
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<WebAdapterClient, IWebAdapterContract>(serviceName);
        }

        public static WebAdapterClient CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<WebAdapterClient, IWebAdapterContract>(serviceUri, servicePartitionKey);
        }

        public Task UpdateGraph(List<NodeViewModel> nodes, List<RelationViewModel> relations)
        {
            return InvokeWithRetryAsync(client => client.Channel.UpdateGraph(nodes, relations));
        }

        public Task UpdateScadaData(Dictionary<long, AnalogModbusData> scadaData)
        {
            throw new NotImplementedException();
        }
    }
}

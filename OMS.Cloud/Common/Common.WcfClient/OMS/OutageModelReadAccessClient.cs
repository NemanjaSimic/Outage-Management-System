using Common.OMS;
using Common.OmsContracts.ModelProvider;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud;
using OMS.Common.Cloud.Names;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.OMS
{
    public class OutageModelReadAccessClient : WcfSeviceFabricClientBase<IOutageModelReadAccessContract>, IOutageModelReadAccessContract
    {
        private static readonly string microserviceName = MicroserviceNames.OmsModelProviderService;
        private static readonly string listenerName = EndpointNames.HistoryDBManagerEndpoint;
        public OutageModelReadAccessClient(WcfCommunicationClientFactory<IOutageModelReadAccessContract> clientFactory,Uri serviceUri,ServicePartitionKey servicePartition)
            :base(clientFactory,serviceUri,servicePartition,listenerName)
        {

        }
        public static OutageModelReadAccessClient CreateClient(Uri serviceUri = null)
        {
            ClientFactory factory = new ClientFactory();
            ServicePartitionKey servicePartition = new ServicePartitionKey(0);
            if(serviceUri == null)
            {
                return factory.CreateClient<OutageModelReadAccessClient, IOutageModelReadAccessContract>(microserviceName, servicePartition);
            }
            else
            {
                return factory.CreateClient<OutageModelReadAccessClient, IOutageModelReadAccessContract>(serviceUri, servicePartition);
            }
        }

        public Task<Dictionary<long, long>> GetCommandedElements()
        {
            return InvokeWithRetryAsync(client => client.Channel.GetCommandedElements());
        }

        public Task<Dictionary<long, long>> GetOptimumIsolatioPoints()
        {
            return InvokeWithRetryAsync(client => client.Channel.GetOptimumIsolatioPoints());
        }

        public Task<Dictionary<long, CommandOriginType>> GetPotentialOutage()
        {
            return InvokeWithRetryAsync(client => client.Channel.GetPotentialOutage());
        }

        #region IOutageModelReadAccessContract
        public Task<IOutageTopologyModel> GetTopologyModel()
        {
            return InvokeWithRetryAsync(client => client.Channel.GetTopologyModel());
        }

        #endregion
    }
}

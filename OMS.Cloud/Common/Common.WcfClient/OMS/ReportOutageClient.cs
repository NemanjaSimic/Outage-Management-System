using Common.OmsContracts.OutageLifecycle;
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
    public class ReportOutageClient : WcfSeviceFabricClientBase<IReportOutageContract>, IReportOutageContract
    {
        private static readonly string microserviceName = MicroserviceNames.OmsOutageLifecycleService;
        private static readonly string listenerName = EndpointNames.ReportOutageEndpoint;
        public ReportOutageClient(WcfCommunicationClientFactory<IReportOutageContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
           : base(clientFactory, serviceUri, servicePartition, listenerName)
        {

        }

        public static ReportOutageClient CreateClient(Uri serviceUri = null)
        {
            ClientFactory factory = new ClientFactory();
            ServicePartitionKey servicePartition = ServicePartitionKey.Singleton;

            if (serviceUri == null)
            {
                return factory.CreateClient<ReportOutageClient, IReportOutageContract>(microserviceName, servicePartition);
            }
            else
            {
                return factory.CreateClient<ReportOutageClient, IReportOutageContract>(serviceUri, servicePartition);
            }
        }
        public Task<bool> ReportPotentialOutage(long gid, CommandOriginType commandOriginType)
        {
            return InvokeWithRetryAsync(client => client.Channel.ReportPotentialOutage(gid, commandOriginType));
        }
    }
}

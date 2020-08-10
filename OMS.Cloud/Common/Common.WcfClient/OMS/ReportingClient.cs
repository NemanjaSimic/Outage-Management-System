using Common.OMS.Report;
using Common.OmsContracts.Report;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.OMS
{
    public class ReportingClient : WcfSeviceFabricClientBase<IReportingContract>, IReportingContract
    {
        private static readonly string microserviceName = MicroserviceNames.OmsHistoryDBManagerService;
        private static readonly string listenerName = EndpointNames.OmsReportingEndpoint;
        public ReportingClient(WcfCommunicationClientFactory<IReportingContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
           : base(clientFactory, serviceUri, servicePartition, listenerName)
        {

        }

        public static ReportingClient CreateClient()
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<ReportingClient, IReportingContract>(microserviceName);
        }

        public static ReportingClient CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<ReportingClient, IReportingContract>(serviceUri, servicePartitionKey);
        }
        public Task<OutageReport> GenerateReport(ReportOptions options)
        {
            return InvokeWithRetryAsync(client => client.Channel.GenerateReport(options));
        }
    }
}

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
        private static readonly string microserviceName = MicroserviceNames.OmsReportingService;
        private static readonly string listenerName = EndpointNames.ReportingEndpoint;
        public ReportingClient(WcfCommunicationClientFactory<IReportingContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
           : base(clientFactory, serviceUri, servicePartition, listenerName)
        {

        }

        public static ReportingClient CreateClient(Uri serviceUri = null)
        {
            ClientFactory factory = new ClientFactory();
            ServicePartitionKey servicePartition = ServicePartitionKey.Singleton;

            if (serviceUri == null)
            {
                return factory.CreateClient<ReportingClient, IReportingContract>(microserviceName, servicePartition);
            }
            else
            {
                return factory.CreateClient<ReportingClient, IReportingContract>(serviceUri, servicePartition);
            }
        }
        public Task<OutageReport> GenerateReport(ReportOptions options)
        {
            return InvokeWithRetryAsync(client => client.Channel.GenerateReport(options));
        }
    }
}

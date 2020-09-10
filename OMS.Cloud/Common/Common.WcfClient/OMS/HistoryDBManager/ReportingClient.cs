using Common.OmsContracts.DataContracts.Report;
using Common.OmsContracts.Report;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Client;
using OMS.Common.Cloud.Names;
using System;
using System.Threading.Tasks;

namespace OMS.Common.WcfClient.OMS.HistoryDBManager
{
    public class ReportingClient : WcfSeviceFabricClientBase<IReportingContract>, IReportingContract
    {
        private static readonly string microserviceName = MicroserviceNames.OmsHistoryDBManagerService;
        private static readonly string listenerName = EndpointNames.OmsReportingEndpoint;

        public ReportingClient(WcfCommunicationClientFactory<IReportingContract> clientFactory, Uri serviceUri, ServicePartitionKey servicePartition)
           : base(clientFactory, serviceUri, servicePartition, listenerName)
        {
        }

        public static IReportingContract CreateClient()
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<ReportingClient, IReportingContract>(microserviceName);
        }

        public static IReportingContract CreateClient(Uri serviceUri, ServicePartitionKey servicePartitionKey)
        {
            ClientFactory factory = new ClientFactory();
            return factory.CreateClient<ReportingClient, IReportingContract>(serviceUri, servicePartitionKey);
        }

        #region IReportingContract
        public Task<OutageReport> GenerateReport(ReportOptions options)
        {
            return InvokeWithRetryAsync(client => client.Channel.GenerateReport(options));
        }

        public Task<bool> IsAlive()
        {
            return InvokeWithRetryAsync(client => client.Channel.IsAlive());
        }
        #endregion IReportingContract
    }
}

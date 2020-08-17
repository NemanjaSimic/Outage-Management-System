using Common.CloudContracts;
using Common.OMS.Report;
using Microsoft.ServiceFabric.Services.Remoting;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Common.OmsContracts.Report
{

    [ServiceContract]
    public interface IReportingContract : IService, IHealthChecker
    {
        [OperationContract]
        Task<OutageReport> GenerateReport(ReportOptions options);
    }
}

using Outage.Common.OutageService;
using Outage.Common.ServiceContracts.OMS;

namespace OutageManagementService.Report
{
    public class ReportingService : IReportingContract
    {
        public OutageReport GenerateReport(ReportOptions options)
        {
            // @TODO:
            // - proveriti koji je report type i generisati odredjeni report

            throw new System.NotImplementedException();
        }
    }
}

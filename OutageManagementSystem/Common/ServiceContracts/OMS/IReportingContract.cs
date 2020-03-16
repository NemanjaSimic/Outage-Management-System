namespace Outage.Common.ServiceContracts.OMS
{
    using Outage.Common.OutageService;

    public interface IReportingContract
    {
        OutageReport GenerateReport(ReportOptions options);
    }
}

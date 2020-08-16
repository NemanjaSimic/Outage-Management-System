using Common.OMS.Report;

namespace OMS.HistoryDBManagerImplementation.Reporting.ReportTypes
{
    public interface IReport
    {
        OutageReport Generate(ReportOptions options);
    }
}

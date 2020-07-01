namespace Outage.Common.OutageService
{
    public interface IReport
    {
        OutageReport Generate(ReportOptions options);
    }
}

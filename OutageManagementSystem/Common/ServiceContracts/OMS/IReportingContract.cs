namespace Outage.Common.ServiceContracts.OMS
{
    using Outage.Common.OutageService;
    using System.ServiceModel;

    [ServiceContract]
    public interface IReportingContract
    {
        [OperationContract]
        OutageReport GenerateReport(ReportOptions options);
    }
}

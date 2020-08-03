namespace Outage.Common.ServiceContracts.OMS
{
    using global::Common.Web.Models.BindingModels;
    using Outage.Common.OutageService;
    using System.ServiceModel;

    [ServiceContract]
    public interface IReportingContract
    {
        [OperationContract]
        OutageReport GenerateReport(ReportOptions options);
    }
}

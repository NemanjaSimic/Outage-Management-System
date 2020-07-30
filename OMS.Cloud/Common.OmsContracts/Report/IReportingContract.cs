using Common.OMS.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common.OmsContracts.Report
{

    [ServiceContract]
    public interface IReportingContract
    {
        [OperationContract]
        Task<OutageReport> GenerateReport(ReportOptions options);
    }
}

using Common.OMS.Report;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common.OmsContracts.Report
{

    [ServiceContract]
    public interface IReportingContract : IService
    {
        [OperationContract]
        Task<OutageReport> GenerateReport(ReportOptions options);
    }
}

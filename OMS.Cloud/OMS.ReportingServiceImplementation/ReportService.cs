using Common.OMS.Report;
using Common.OmsContracts.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.ReportingServiceImplementation
{
    public class ReportService : IReportingContract
    {
        public Task<OutageReport> GenerateReport(ReportOptions options)
        {
            throw new NotImplementedException();
        }
    }
}

using Common.OMS.Report;
using Common.OmsContracts.Report;
using OMS.Common.Cloud;
using OMS.HistoryDBManagerServiceImplementation.Reporting.ReportTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.HistoryDBManagerServiceImplementation
{
    public class ReportService : IReportingContract
    {
        public IDictionary<ReportType, Func<IReport>> reports
            = new Dictionary<ReportType, Func<IReport>>
            {
                { ReportType.Total, () => new TotalReport() },
                { ReportType.SAIFI, () => new SaifiReport() },
                { ReportType.SAIDI, () => new SaidiReport() }
            };

        public async Task<OutageReport> GenerateReport(ReportOptions options)
        {
            return reports[options.Type]().Generate(options);
        }
    }
}

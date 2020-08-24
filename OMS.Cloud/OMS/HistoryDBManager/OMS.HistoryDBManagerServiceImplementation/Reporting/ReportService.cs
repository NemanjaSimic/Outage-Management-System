using Common.OmsContracts.DataContracts.Report;
using Common.OmsContracts.Report;
using OMS.Common.Cloud;
using OMS.HistoryDBManagerImplementation.Reporting.ReportTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OMS.HistoryDBManagerImplementation.Reporting
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
        public Task<bool> IsAlive()
        {
            return Task.Run(() => { return true; });
        }
    }
}

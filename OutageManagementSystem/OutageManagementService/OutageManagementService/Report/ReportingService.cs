using Outage.Common.OutageService;
using Outage.Common.ServiceContracts.OMS;
using System;
using System.Collections.Generic;

namespace OutageManagementService.Report
{
    public class ReportingService : IReportingContract
    {
        public IDictionary<ReportType, Func<IReport>> reports
            = new Dictionary<ReportType, Func<IReport>>
            {
                { ReportType.Total, () => new TotalReport() },
                { ReportType.SAIFI, () => new SaifiReport() },
                { ReportType.SAIDI, () => new SaidiReport() }
            };

        public OutageReport GenerateReport(ReportOptions options)
            => reports[options.Type]().Generate(options);
    }
}

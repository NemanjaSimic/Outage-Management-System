using Common.OMS.Report;
using Common.OmsContracts.Report;
using OMS.Common.Cloud;
using OMS.ReportingServiceImplementation.ReportTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.ReportingServiceImplementation
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
            return  reports[options.Type]().Generate(options);
        }
        public Task<bool> IsAlive()
        {
            return Task.Run(() => { return true; });
        }
    }
}

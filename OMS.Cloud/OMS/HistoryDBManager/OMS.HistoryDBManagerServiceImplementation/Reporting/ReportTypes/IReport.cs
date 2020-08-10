using Common.OMS.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMS.HistoryDBManagerServiceImplementation.Reporting.ReportTypes
{
    public interface IReport
    {
        OutageReport Generate(ReportOptions options);
    }
}

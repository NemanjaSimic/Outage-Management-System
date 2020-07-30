using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.OMS.Report
{
    public interface IReport
    {
        OutageReport Generate(ReportOptions options);
    }
}

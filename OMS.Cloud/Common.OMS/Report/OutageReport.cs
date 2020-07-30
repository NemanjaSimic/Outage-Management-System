using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.OMS.Report
{
    public class OutageReport
    {
        public IDictionary<string, float> Data { get; set; }
        public string Type { get; set; }
    }
}

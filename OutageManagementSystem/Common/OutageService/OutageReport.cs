using System.Collections.Generic;

namespace Outage.Common.OutageService
{
    public class OutageReport
    {
        public IDictionary<string, int> Data { get; set; }
        public string Type { get; set; }
    }
}

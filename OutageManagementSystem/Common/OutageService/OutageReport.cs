using System.Collections.Generic;

namespace Outage.Common.OutageService
{
    public class OutageReport
    {
        public IDictionary<string, float> Data { get; set; }
        public string Type { get; set; }
    }
}

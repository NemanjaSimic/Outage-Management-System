using System.Collections.Generic;

namespace Common.Web.Models.ViewModels
{
    public class ReportViewModel : IViewModel
    {
        public IDictionary<string, float> Data { get; set; }
        public string Type { get; set; }
    }
}

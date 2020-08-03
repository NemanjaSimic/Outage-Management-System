using System;

namespace Common.Web.Models.BindingModels
{
    public class ReportOptions
    {
        public ReportType Type { get; set; }
        public long? ElementId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}

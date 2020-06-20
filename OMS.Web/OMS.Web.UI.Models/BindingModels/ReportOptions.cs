namespace OMS.Web.UI.Models.BindingModels
{   
    using System;

    public class ReportOptions
    {
        public ReportType Type { get; set; }
        public long? ElementId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}

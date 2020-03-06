namespace OMS.Web.UI.Models.ViewModels
{
    using System.Collections.Generic;

    public class ReportViewModel
    {
        public Dictionary<string, int> Data { get; set; }
        public string Type { get; set; } // Yearly, Monthly
    }
}

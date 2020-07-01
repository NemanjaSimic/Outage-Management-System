namespace OMS.Web.UI.Models.ViewModels
{
    using System.Collections.Generic;

    public class ReportViewModel : IViewModel
    {
        public IDictionary<string, float> Data { get; set; }
        public string Type { get; set; }
    }
}
